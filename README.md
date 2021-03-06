# OSM to SHP Fastex Converter

Flexible OSM2SHP Fastex Converter (convert .osm &amp; .pbf files to ESRI Shape .shp files and .dbf)   
Настраиваемая программа-конвертер файлов данных OSM (*.osm) и (*.pbf) в ESRI Shapes (*.shp и .dbf) 

Возможности:
- Создание по отдельности файлов с точками, полилиниями, полигонами и связями между объектами
- Обработка контуров (полилиний и полигонов) с использованием фильтров, а также центроидов, адресов, дорог и зданий
- Выделение адресной информации (сохранение адресных тегов в соответствующие поля dbf файла)
- Учитываются ограничения на количество объектов и размера данных в dbf файле
- Выбор кодировки, основных и дополнительных полей dbf файла с вохможностью указания размера каждого поля
- Возможность сохранения всех тегов (атрибутивной информации) для каждого объекта в dbf файл
- Обработка центроидов полигонов и сохранение их в виде точек со всей атрибутивной информацией (тегами)
- Сохранение срединных узлов полилиний в shape файл (для обработки запретов поворотов и разбиения полилиний на составляющие)
- Таблица с полигонами может быть использована для создания маршрутного графа (построение маршрутов с использованием полученных через конвертер данных протестировано на нескольких странах/регионах)
- Поддерживается сохранение запретов поворотов в формате Garmin (автоматически проставляется в соответствующие поля dbf файла) и OSM
- Сохранение номеров крайних точек полилинии в dbf файл (может быть использовано для запретов поворотов)
- Обработка связей между объектами в качестве полилинии или полигона
- Можно использовать как готовые пресеты (POI, MP, Адреса) для конвертации, так и создавать свои
- Обработка POI с использованием пресетов в виде JSON и XML шаблонов
- В качестве готовых пресетов можно использовать адресный селектор, который помещает адресную информацию в соответствующие поля dbf файла   
- Готовый адресный селектор для русских [ru] и анлийских [en] адресных тегов
- Настраиваемый адресный селектор для собственных адресных тегов [custom]
- Можно агрегировать все теги объекта в одно или несколько полей. Либо выделить какой-либо теги в отдельную колонку
- Можно агрегировать только определенные или неопределенные теги
- Сохранение агрегированных тегов в порядке приоритета
- Фильтрование объектов на основе тегов, условий и с помощью скрипта на C#
- Фильтрование объектов на основе ограничивающего прямоугольника
- Фильтрование объектов на основе ограничивающего полигона
- Визуализация процесса конвертации через HTTP-интерфейс (веб-сервер)
- Создание подробного отчета
- Запуск на слабых машинах и XP

### Get OpenStreetMap DATA - Данные OpenStreetMap

[OpenStreetMap](https://www.openstreetmap.org/export) - OSM        
[OSM Planet in PBF format](https://planet.openstreetmap.org/pbf/)     
[OSM Planet in OSM format](https://planet.openstreetmap.org/planet/)    
[GeoFabric PBF+OSM format](https://download.geofabrik.de/)    
[BBBike](https://extract.bbbike.org/), use:  Protocolbuffer (PBF) file format       
[BBBike Extracted](https://download.bbbike.org/osm/extract/), use:  Protocolbuffer (PBF) file format   

<img src="window1.png"/>
<img src="window2.png"/>
<img src="window3.png"/>
<img src="window4.png"/>
<img src="window5.png"/>
<img src="window6.png"/>
<img src="window7.png"/>
<img src="window8.png"/>
<img src="window9.png"/>
<img src="windowA.png"/>
<img src="windowB.png"/>
<img src="windowC.png"/>

<img src="wind2_01.png"/>
<img src="wind2_02.png"/>
<img src="wind2_03.png"/>
<img src="wind2_04.png"/>
<img src="wind2_05.png"/>
<img src="wind2_06.png"/>
<img src="wind2_07.png"/>
<img src="wind2_08.png"/>
<img src="wind2_09.png"/>
