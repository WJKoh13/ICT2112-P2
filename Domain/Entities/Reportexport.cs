using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Reportexport
{
    private int _reportid;
    private int Reportid { get => _reportid; set => _reportid = value; }

    private int? _refanalyticsid;
    private int? Refanalyticsid { get => _refanalyticsid; set => _refanalyticsid = value; }

    private string? _title;
    private string? Title { get => _title; set => _title = value; }

    private string? _url;
    private string? Url { get => _url; set => _url = value; }

    public virtual Analytic? Refanalytics { get; private set; }
}
