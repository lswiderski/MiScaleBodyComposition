# MiScaleBodyComposition

Lib for calculate the body composition from data returned from Mi Body Composition Scale

## Instalation

- dotnet cli

`dotnet add package MiScaleBodyComposition`

- Package Manager

`Install-Package MiScaleBodyComposition`

## Usage

```csharp
using MiScaleBodyComposition;
```

```csharp
var data = new byte[] {2,166,230,7,2,11,17,34,7,186,1,60,55};

var result = new MiScale().GetBodyComposition(data, new User(175, 25, Sex.Male));
```

## Byte Array description

Mine Mi Body Composition Scale return 15 bytes long array. Some other versions of scale can return 17 byte long array.

Only last 13 bytes are the important payload:

- bytes 0 and 1: control bytes
- bytes 2 and 3: year
- byte 4: month
- byte 5: day
- byte 6: hours
- byte 7: minutes
- byte 8: seconds
- bytes 9 and 10: impedance
- bytes 11 and 12: weight (*100 for pounds and catty, *200 for kilograms)

(source: https://github.com/wiecosystem/Bluetooth/blob/master/doc/devices/huami.health.scale2.md)

## Used in

https://github.com/lswiderski/mi-scale-exporter

## Inspiration

- https://github.com/RobertWojtowicz/miscale2garmin
- https://github.com/lolouk44/xiaomi_mi_scale
- https://github.com/rando-calrissian/esp32_xiaomi_mi_2_hass


## Coffee

<a href="https://www.buymeacoffee.com/lukaszswiderski" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" height="41" width="174"></a>
