using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class ShippingOption
{
    private int _optionId;
    private int OptionId { get => _optionId; set => _optionId = value; }

    private string? _displayName;
    private string? DisplayName { get => _displayName; set => _displayName = value; }

    private decimal? _cost;
    private decimal? Cost { get => _cost; set => _cost = value; }

    private double? _carbonfootprintkg;
    private double? Carbonfootprintkg { get => _carbonfootprintkg; set => _carbonfootprintkg = value; }

    private int? _deliveryDays;
    private int? DeliveryDays { get => _deliveryDays; set => _deliveryDays = value; }

    private int? _orderId;
    private int? OrderId { get => _orderId; set => _orderId = value; }

    private int? _routeId;
    private int? RouteId { get => _routeId; set => _routeId = value; }

    public virtual ICollection<Checkout> Checkouts { get; private set; } = new List<Checkout>();

    public virtual Order? Order { get; private set; }

    public virtual DeliveryRoute? Route { get; private set; }
}
