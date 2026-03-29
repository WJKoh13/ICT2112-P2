using ProRental.Domain.Enums;
using ProRental.Models.Module3.P2_1;

namespace ProRental.Interfaces.Module3.P2_1;

/// <summary>
/// Feature 1 preference-context boundary. It resolves the customer-facing
/// checkout preference cards and the selected preference definition.
/// by: ernest
/// </summary>
public interface IShippingPreferenceService
{
    IReadOnlyList<ShippingPreferenceCard> BuildPreferenceCards(
        CheckoutShippingContext context,
        bool isSameCountry);

    ResolvedShippingPreference ResolvePreference(
        PreferenceType preferenceType,
        bool isSameCountry);
}
