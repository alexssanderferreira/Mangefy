using Mangefy.Application.Common.Exceptions;
using Mangefy.Application.Common.Interfaces;
using Mangefy.Domain.Menus;
using Mangefy.Domain.Menus.Repositories;
using Mangefy.Domain.Platform.Features;
using MediatR;

namespace Mangefy.Application.Menus.Commands.SetMenuItemRecipe;

public sealed class SetMenuItemRecipeCommandHandler : IRequestHandler<SetMenuItemRecipeCommand>
{
    private readonly IMenuRepository _menus;
    private readonly IUnitOfWork _uow;
    private readonly IFeatureGateService _featureGate;

    public SetMenuItemRecipeCommandHandler(IMenuRepository menus, IUnitOfWork uow, IFeatureGateService featureGate)
    {
        _menus = menus;
        _uow = uow;
        _featureGate = featureGate;
    }

    public async Task Handle(SetMenuItemRecipeCommand request, CancellationToken cancellationToken)
    {
        await _featureGate.RequireAsync(request.TenantId, FeatureCatalog.Stock.Basic, cancellationToken);

        var menu = await _menus.GetByIdAsync(request.MenuId, cancellationToken)
            ?? throw new NotFoundException(nameof(Menu), request.MenuId);

        if (menu.TenantId != request.TenantId)
            throw new ForbiddenException();

        var ingredients = request.Ingredients
            .Select(i => RecipeIngredient.Create(i.StockItemId, i.Quantity))
            .ToList();

        menu.SetItemRecipe(request.CategoryId, request.ItemId, ingredients);

        await _menus.UpdateAsync(menu, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
