using API.Hubs;
using Data.Repositories;
using Domain.Models;
using Domain.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace API.Services
{
    public class VoteStatusBroadcastService(
        IServiceProvider serviceProvider,
        IHubContext<VoteStatusHub> hubContext,
        ILogger<VoteStatusBroadcastService> logger
    ) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IHubContext<VoteStatusHub> _hubContext = hubContext;
        private readonly ILogger<VoteStatusBroadcastService> _logger = logger;
        private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(5);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Vote Status Broadcast Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await BroadcastVoteStatus();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error broadcasting vote status");
                }

                await Task.Delay(_updateInterval, stoppingToken);
            }

            _logger.LogInformation("Vote Status Broadcast Service stopped.");
        }

        private async Task BroadcastVoteStatus()
        {
            using var scope = _serviceProvider.CreateScope();
            var candidacyTypeRepository = scope.ServiceProvider.GetRequiredService<CandidacyTypeRepository>();
            var scrutinyRepository = scope.ServiceProvider.GetRequiredService<ScrutinyRepository>();
            var slateRepository = scope.ServiceProvider.GetRequiredService<SlateRepository>();
            var slateCandidacyRepository = scope.ServiceProvider.GetRequiredService<SlateCandidacyRepository>();
            var voteRepository = scope.ServiceProvider.GetRequiredService<VoteRepository>();

            var now = DateTime.Now;

            var openScrutinies = await scrutinyRepository.GetAllNoPagination(
                scrutiny => scrutiny.StatusId == EScrutinyStatus.OPEN.GetValue() && scrutiny.EndDate > now,
                null
            );

            var scrutiniesStatus = new List<ScrutinyVoteStatus>();

            var firstCandidacyTypePostion = await candidacyTypeRepository.GetOneByFilter(
                orderByAscending: orderBy => orderBy.Position
            );

            if (firstCandidacyTypePostion == null)
            {
                throw new Exception("Must have a least one candidacy type");
            }

            foreach (var scrutiny in openScrutinies)
            {
                var slates = await slateRepository.GetAllNoPagination(
                    slate => slate.ScrutinyId == scrutiny.Id,
                    null
                );

                var slatesStatus = new List<SlateVoteStatus>();

                foreach (var slate in slates)
                {
                    var voteCount = await voteRepository.Count(vote => vote.SlateId == slate.Id);

                    var firstCandidacy = await slateCandidacyRepository.GetOneByFilter(
                        filter: candidacy => candidacy.SlateId == slate.Id && candidacy.CandidacyTypeId == firstCandidacyTypePostion.Id,
                        include: "CandidacyType"
                    );

                    slatesStatus.Add(new SlateVoteStatus
                    {
                        Id = slate.Id,
                        Position = slate.Position,
                        VoteCount = voteCount,
                        FirstCandidacy = firstCandidacy != null ? new FirstCandidacy
                        {
                            Id = firstCandidacy.Id,
                            Name = firstCandidacy.Name,
                            LastName = firstCandidacy.LastName,
                            ImageUrl = firstCandidacy.ImageUrl
                        } : null
                    });
                }

                var totalVotes = await voteRepository.Count(vote => vote.ScrutinyId == scrutiny.Id);

                scrutiniesStatus.Add(new ScrutinyVoteStatus
                {
                    Id = scrutiny.Id,
                    Title = scrutiny.Title,
                    Description = scrutiny.Description,
                    StartDate = scrutiny.StartDate,
                    EndDate = scrutiny.EndDate,
                    ImageUrl = scrutiny.ImageUrl,
                    TotalVotes = totalVotes,
                    Slates = slatesStatus
                });
            }

            var update = new VoteStatusUpdate
            {
                Scrutinies = scrutiniesStatus
            };

            await _hubContext.Clients.Group("VoteUpdates").SendAsync("ReceiveVoteStatus", update);
        }
    }
}
