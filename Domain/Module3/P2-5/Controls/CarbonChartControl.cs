using ProRental.Data.Module3.P2_5.Interfaces;
using ProRental.Domain.Module3.P2_5.Entities;
using ProRental.Interfaces.Module3.P2_5;

namespace ProRental.Domain.Module3.P2_5.Controls;

public sealed class CarbonChartControl : ICarbonChartService
{
    private readonly IBuildingFootprintTableGateway _buildingFootprintTableGateway;
    private readonly IProductFootprintTableGateway _productFootprintTableGateway;

    public CarbonChartControl(
        IBuildingFootprintTableGateway buildingFootprintTableGateway,
        IProductFootprintTableGateway productFootprintTableGateway)
    {
        _buildingFootprintTableGateway = buildingFootprintTableGateway;
        _productFootprintTableGateway = productFootprintTableGateway;
    }

    public List<ChartData> Hotspots { get; private set; } = [];

    public List<ChartData> CreateCharts()
    {
        return _buildingFootprintTableGateway.GetHourlyChartData();
    }

    public List<ChartData> CreateGraphs()
    {
        return _buildingFootprintTableGateway.GetZoneGraphData();
    }

    public void IdentifyHotspots(string groupBy)
    {
        Hotspots = _buildingFootprintTableGateway.GetHotspotData(groupBy);
    }

    public List<ChartData> GetHotspots()
    {
        return Hotspots;
    }

    public CarbonDashboardViewModel BuildDashboardViewModel()
    {
        IdentifyHotspots("room");

        var productGraphData = _productFootprintTableGateway.GetProductGraphData();
        var buildingGraphData = CreateGraphs();

        return new CarbonDashboardViewModel
        {
            ProductTrendline = _productFootprintTableGateway.GetHourlyChartData(),
            ProductBarChart = productGraphData,
            ProductPieChart = productGraphData,
            BuildingTrendline = CreateCharts(),
            BuildingBarChart = buildingGraphData,
            BuildingPieChart = buildingGraphData,
            Hotspots = GetHotspots()
        };
    }
}
