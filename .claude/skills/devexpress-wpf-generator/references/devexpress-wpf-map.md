# DevExpress WPF Map Control

## Overview
- Geographic/Cartesian map control with Azure Maps, OSM, Mapbox, MbTiles providers.
- ImageLayer (tiles), VectorLayer (shapes), InformationLayer (GIS).
- KML, Shapefile, SVG, GeoJSON import, clustering, measurement tools.

## Key Classes
| Class | Description |
|-------|-------------|
| `MapControl` | Main map control |
| `ImageLayer` | Tile image layer |
| `VectorLayer` | Vector shape layer |
| `OpenStreetMapDataProvider` | OSM tile provider |
| `MapPushpin` | Pushpin marker |
| `MapPolygon` / `MapPolyline` | Shape elements |

- **xmlns**: `dxm="http://schemas.devexpress.com/winfx/2008/xaml/map"`
- **NuGet**: `DevExpress.Wpf.Map`

## Basic Map with OSM
```xml
<dxm:MapControl CenterPoint="37.5665,126.9780" ZoomLevel="12">
    <dxm:ImageLayer>
        <dxm:OpenStreetMapDataProvider/>
    </dxm:ImageLayer>
    <dxm:VectorLayer>
        <dxm:MapItemStorage>
            <dxm:MapPushpin Location="37.5665,126.9780" Text="Seoul"/>
        </dxm:MapItemStorage>
    </dxm:VectorLayer>
</dxm:MapControl>
```

## Data-Bound Pushpins
```xml
<dxm:VectorLayer>
    <dxm:ListSourceDataAdapter DataSource="{Binding Locations}"
        LatitudeMember="Lat" LongitudeMember="Lon">
        <dxm:ListSourceDataAdapter.ItemTemplate>
            <DataTemplate>
                <dxm:MapPushpin Text="{Binding Name}"/>
            </DataTemplate>
        </dxm:ListSourceDataAdapter.ItemTemplate>
    </dxm:ListSourceDataAdapter>
</dxm:VectorLayer>
```

## Reference
- https://docs.devexpress.com/WPF/17905/controls-and-libraries/map-control
