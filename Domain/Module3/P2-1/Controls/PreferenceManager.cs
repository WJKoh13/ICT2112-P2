using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;
using ProRental.Models.Module3.P2_1;

namespace ProRental.Domain.Controls;

/// <summary>
/// Feature 1 strategy context. It owns all checkout preference-specific variation
/// and delegates allowed-mode resolution to the registered concrete strategies.
/// by: ernest
/// </summary>
internal sealed class PreferenceManager : IShippingPreferenceService
{
    private static readonly ShippingPreferenceMetadata[] PreferenceMetadata =
    [
        new(
            PreferenceType.FAST,
            1,
            "Fastest",
            "Best for time-sensitive deliveries when you want the quickest available route.",
            1),
        new(
            PreferenceType.CHEAP,
            2,
            "Cheapest",
            "Best value when keeping delivery costs low matters more than speed.",
            5),
        new(
            PreferenceType.GREEN,
            3,
            "Greenest",
            "A balanced delivery choice that keeps the journey efficient while reducing transport impact.",
            4)
    ];

    private static readonly IReadOnlyDictionary<PreferenceType, ShippingPreferenceMetadata> PreferenceMetadataByType =
        PreferenceMetadata.ToDictionary(metadata => metadata.PreferenceType);

    private readonly IReadOnlyDictionary<PreferenceType, IShippingPreferenceStrategy> _strategies;

    public PreferenceManager(IEnumerable<IShippingPreferenceStrategy> strategies)
    {
        _strategies = CreateStrategyMap(strategies);
    }

    public IReadOnlyList<ShippingPreferenceCard> BuildPreferenceCards(
        CheckoutShippingContext context,
        bool isSameCountry)
    {
        ArgumentNullException.ThrowIfNull(context);

        return PreferenceMetadata
            .OrderBy(metadata => metadata.DisplayPreference)
            .Select(metadata => BuildCard(context, ResolvePreference(metadata.PreferenceType, isSameCountry)))
            .ToArray();
    }

    public ResolvedShippingPreference ResolvePreference(
        PreferenceType preferenceType,
        bool isSameCountry)
    {
        var strategy = GetStrategy(preferenceType);
        var metadata = GetMetadata(preferenceType);
        var allowedModes = strategy.ResolveAllowedModes(isSameCountry).ToArray();

        if (allowedModes.Length == 0)
        {
            throw new InvalidOperationException(
                $"Shipping preference '{preferenceType}' did not resolve any transport modes.");
        }

        return new ResolvedShippingPreference(
            metadata.PreferenceType,
            metadata.DisplayPreference,
            metadata.DisplayName,
            metadata.Description,
            metadata.DeliveryDays,
            allowedModes);
    }

    private static ShippingPreferenceCard BuildCard(
        CheckoutShippingContext context,
        ResolvedShippingPreference preference)
    {
        var allowedModesLabel = string.Join(" + ", preference.AllowedModes);

        return new ShippingPreferenceCard(
            context.CheckoutId,
            preference.PreferenceType,
            preference.DisplayName,
            preference.Description,
            allowedModesLabel);
    }

    private static IReadOnlyDictionary<PreferenceType, IShippingPreferenceStrategy> CreateStrategyMap(
        IEnumerable<IShippingPreferenceStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);

        var strategyMap = new Dictionary<PreferenceType, IShippingPreferenceStrategy>();
        foreach (var strategy in strategies)
        {
            ArgumentNullException.ThrowIfNull(strategy);
            if (!strategyMap.TryAdd(strategy.PreferenceType, strategy))
            {
                throw new InvalidOperationException(
                    $"Duplicate shipping preference strategy registration for '{strategy.PreferenceType}'.");
            }
        }

        var missingPreferences = PreferenceMetadataByType.Keys
            .Where(preferenceType => !strategyMap.ContainsKey(preferenceType))
            .OrderBy(preferenceType => preferenceType)
            .ToArray();

        if (missingPreferences.Length > 0)
        {
            throw new InvalidOperationException(
                $"Missing shipping preference strategies: {string.Join(", ", missingPreferences)}.");
        }

        return strategyMap;
    }

    private static ShippingPreferenceMetadata GetMetadata(PreferenceType preferenceType)
    {
        if (PreferenceMetadataByType.TryGetValue(preferenceType, out var metadata))
        {
            return metadata;
        }

        throw new InvalidOperationException($"No shipping preference metadata was configured for '{preferenceType}'.");
    }

    private IShippingPreferenceStrategy GetStrategy(PreferenceType preferenceType)
    {
        if (_strategies.TryGetValue(preferenceType, out var strategy))
        {
            return strategy;
        }

        throw new InvalidOperationException($"No shipping preference strategy registered for '{preferenceType}'.");
    }

    private sealed record ShippingPreferenceMetadata(
        PreferenceType PreferenceType,
        int DisplayPreference,
        string DisplayName,
        string Description,
        int DeliveryDays);
}
