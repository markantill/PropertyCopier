# PropertyCopier
C# class to automatically copy properties between objects by creating expression trees to copy the properties, usable in LINQ queries.

* Copies properties to new objects
* Copies properties into existing objects
* Can copy child objects
* Can flatten objects
* Can recursively copy child objects and enumerations
* Can create copy expression for use in Linq queries
* Creates expressions and functions once for types involved and caches result.
* Casts properties to target type where possible

# Examples

Copy to new object:

    var entity = new EnitiyOne() { ID = 10 }
    var dto = Copy.PropertiesFrom(entity).ToNew<DtoOne>();

Copy to existing object:

    var dto = new DtoOne { ID = 0 };
    Copy.PropertiesFrom(new EnitiyOne() { ID = 10 }).ToExisting(dto);

