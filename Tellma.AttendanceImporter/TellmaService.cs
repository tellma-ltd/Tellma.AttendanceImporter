using Microsoft.Extensions.Options;
using Tellma.Api.Dto;
using Tellma.AttendanceImporter.Contract;
using Tellma.Client;
using Tellma.Model.Application;

namespace Tellma.AttendanceImporter
{
    internal class TellmaService : ITellmaService
    {
        private readonly TellmaClient _client; // wrapper calling Tellma server => client
        public TellmaService(IOptions<TellmaOptions> options)
        {
            // Create the client
            _client = new TellmaClient(
                baseUrl: "https://web.tellma.com",
                authorityUrl: "https://web.tellma.com",
                clientId: options.Value.ClientId,
                clientSecret: options.Value.ClientSecret);
        }
        public async Task<IEnumerable<DeviceInfo>> GetDeviceInfos(int tenantId, CancellationToken token)
        {
            var tenantClient = _client.Application(tenantId);
            var deviceDefinitionResult = await tenantClient
                .ResourceDefinitions
                .GetEntities(new GetArguments
                {
                    Filter = "Code = 'TimeAndAttendanceDevices'"
                }, token);

            if (deviceDefinitionResult.Data.Count == 0)
            {
                // TODO: Add log warning
                return Enumerable.Empty<DeviceInfo>();
            }
            var syncResult = await tenantClient
                .DetailsEntries
                .GetAggregate(new GetAggregateArguments
                {
                    Select = "NotedResourceId, MAX(Time1)",
                    Filter = "Line.Definition.Code = 'ToHRAttendanceLog.E'"
                }, token);
            var syncDictionary = syncResult
                .Data
                .ToDictionary(row => Convert.ToInt32(row[0]), row => (DateTime)row[1]);

            var devicesResult = await tenantClient
                .Resources(deviceDefinitionResult.Data[0].Id)
                .GetEntities(new GetArguments
                {
                    Expand = "Lookup1", // brings all resource properties and lookup1 props.
                    Filter = "IsActive = true"
                }, token);

            var deviceInfos = devicesResult
                .Data
                .Where(d =>
                    d.Lookup1 != null &&
                    !string.IsNullOrWhiteSpace(d.Lookup1.Code)
                    )
                .Select(d => new DeviceInfo(deviceType: d.Lookup1.Code)
                {
                    Id = d.Id,
                    Name = d.Name,
                    DutyStationId = d.Agent1Id,
                    IpAddress = d.Text1,
                    Port = d.Int1,
                    // assume new devices have 1970-01-01 last sync
                    LastSyncTime = syncDictionary.GetValueOrDefault(d.Id, new DateTime(1970, 1, 1))
                });

            return deviceInfos;
        }

        public async Task Import(int tenantId, IEnumerable<AttendanceRecord> records, CancellationToken token)
        {
            var tenantClient = _client.Application(tenantId);
            var documentDefinitionResult = await tenantClient
              .DocumentDefinitions
              .GetEntities(new GetArguments
              {
                  Filter = "Code = 'EmployeeAttendanceRegister'"
              }, token);

            if (documentDefinitionResult.Data.Count == 0)
            {
                throw new Exception($"Tenant id {tenantId} does not have document definition with code: EmployeeAttendanceRegister");
            }
            var documentDefinitionId = documentDefinitionResult.Data[0].Id;

            var lineDefinitionResult = await tenantClient
              .LineDefinitions
              .GetEntities(new GetArguments
              {
                  Filter = "Code = 'ToHRAttendanceLog.E'"
              }, token);

            if (lineDefinitionResult.Data.Count == 0)
            {
                throw new Exception($"Tenant id {tenantId} does not have line definition with code: ToHRAttendanceLog.E");
            }
            var lineDefinitionId = lineDefinitionResult.Data[0].Id;

            // records brought already are after last sync date
            foreach (var recordGroup in records
                .GroupBy(r => new { r.DeviceInfo.DutyStationId, r.Time.Date })
            )
            {
                var date = recordGroup.Key.Date;
                var dutyStationId = recordGroup.Key.DutyStationId;

                var docResults = await tenantClient
                .Documents(documentDefinitionId)
                .GetEntities(new GetArguments
                {
                    Filter = $"PostingDate = '{date:yyyy-MM-dd}' && NotedAgentId = {dutyStationId} && State >= 0"
                }, token);

                DocumentForSave documentForSave;
                if (docResults.Data.Count > 0)
                {
                    var doc = docResults.Data[0];
                    if (doc.State == 1)
                    {// open the document
                        await tenantClient
                            .Documents(documentDefinitionId)
                            .Open(new List<int>
                            {
                                doc.Id
                            }, cancellation: token);
                    }
                    documentForSave = await tenantClient
                        .Documents(documentDefinitionId)
                        .GetByIdForSave(doc.Id, null, token);
                }
                else
                {
                    documentForSave = new DocumentForSave()
                    {
                        // Document Properties
                        PostingDate = date,
                        PostingDateIsCommon = true,
                        NotedAgentId = dutyStationId,
                        NotedAgentIsCommon = true,
                        Lines = new List<LineForSave>()
                    };
                }
                foreach (var record in recordGroup)
                {
                    documentForSave.Lines.Add(
                        new LineForSave
                        {
                            DefinitionId = lineDefinitionId,
                            Boolean1 = record.IsIn,
                            Text1 = record.Time.ToString("HH:mm:ss"),
                            Text2 = record.UserId,
                            Entries = new List<EntryForSave>
                            {
                                new EntryForSave
                                {
                                    NotedResourceId = record.DeviceInfo.Id
                                }
                            }

                        });

                }
                await tenantClient
                    .Documents(documentDefinitionId)
                    .Save(documentForSave, cancellation: token);
            }
        }
    }
}
