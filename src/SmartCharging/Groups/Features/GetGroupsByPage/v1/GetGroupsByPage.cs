using SmartCharging.Groups.Dtos;
using SmartCharging.Shared.Application.Data;
using SmartCharging.Shared.BuildingBlocks.Extensions;

namespace SmartCharging.Groups.Features.GetGroupsByPage.v1;

public sealed record GetGroupsByPage(int PageNumber = 1, int PageSize = 5)
{
    public static GetGroupsByPage Of(int pageNumber = 1, int pageSize = 5)
    {
        return new GetGroupsByPage(pageNumber.NotBeNegativeOrZero(), pageSize.NotBeNegativeOrZero());
    }
};

public sealed class GetGroupsByPageHandler(IUnitOfWork unitOfWork)
{
    public async Task<GetGroupsByPageResult> Handle(
        GetGroupsByPage getGroupsByPage,
        CancellationToken cancellationToken
    )
    {
        getGroupsByPage.NotBeNull();

        // Fetch groups and total count
        var (groups, totalCount) = await unitOfWork.GroupRepository.GetByPageAndTotalCountAsync(
            getGroupsByPage.PageNumber,
            getGroupsByPage.PageSize,
            cancellationToken
        );

        // Calculate page count
        var pageSize = getGroupsByPage.PageSize;
        var pageCount = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Return the paginated result
        return new GetGroupsByPageResult(
            groups.Select(group => group.ToGroupDto()).ToList().AsReadOnly(),
            pageSize,
            pageCount,
            totalCount
        );
    }
}

public sealed record GetGroupsByPageResult(
    IReadOnlyCollection<GroupDto> Groups,
    int PageSize,
    int PageCount,
    int TotalCount
);
