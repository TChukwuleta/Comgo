using Comgo.Application.Common.Interfaces;
using Comgo.Core.Entities;
using Comgo.Core.Model;
using MediatR;

namespace Comgo.Application.PSBTs.Commands
{
    public class CreatePSBTRecordCommand : IRequest<Result>
    {
        public string InitialPSBT { get; set; }
        public string UserSignedPSBT { get; set; }
        public bool ShouldProcessPSBT { get; set; }
        public string Reference { get; set; }
        public string UserId { get; set; }
    }

    public class CreatePSBTRecordCommandHandler : IRequestHandler<CreatePSBTRecordCommand, Result>
    {
        private readonly IAppDbContext _context;
        public CreatePSBTRecordCommandHandler(IAppDbContext context)
        {
            _context = context; 
        }
        public async Task<Result> Handle(CreatePSBTRecordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var newPSBT = new PSBT
                {
                    UserId = request.UserId,
                    Reference = request.Reference,
                    Status = Core.Enums.Status.Active,
                    CreatedDate = DateTime.Now,
                    ShouldProcessPSBT = request.ShouldProcessPSBT,
                    UserSignedPSBT = request.UserSignedPSBT
                };
                await _context.PSBTs.AddAsync(newPSBT);
                await _context.SaveChangesAsync(cancellationToken);
                return Result.Success("PSBT record created successfully");
            }
            catch (Exception ex)
            {
                return Result.Failure($"An error occured while creating PSBT record. {ex.Message}");
            }
        }
    }
}
