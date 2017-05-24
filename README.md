# PropertyCopier
C# class to automatically copy properties between objects by creating expression trees to copy the properties, usable in LINQ queries.
Why make such a class when things like AutoMapper already exist? I needed some specific properties that it didn't provide.

* Works in .net 4.0 and up
* Provides instanced based mappers with different rules
* Map classes with default rules without having to set anything up.
* Copies properties to new objects
* Copies properties into existing objects
* Can copy child objects
* Can flatten objects
* Can recursively copy child objects, enumerations and collections
* Can create copy expression for use in Linq queries
* Creates expressions and functions once for types involved and caches result.
* Casts properties to target type where possible

Can set up a variety of rules on an instance based copier

* Name mapping rules using StringComparer, choose case sensitive, culture, ordinal or custom
* Map only value types and strings or everything.
* Ignore properties
* Map one name to another
* Provide specific rule for specific properties
* Provide specific rule for types

# Examples

Static copier with default rules.

Copy to new object:

    var entity = new EnitiyOne() { ID = 10 }
    var dto = Copy.From(entity).To<DtoOne>();

Copy to existing object:

    var dto = new DtoOne { ID = 0 };
    Copy.From(new EnitiyOne() { ID = 10 }).To(dto);
	
Copy every object in a collection:

	var list = new List<EntityOne>(setOfItems);
	var result = list.Copy().To<DtoOne>();
	
Instance based copier.

Set up rules:
	var copier = new Copier();	
	copier.SetMapping<EntityOne, DtoOne>(comparer: StringComparer.InvariantCulture, scalarOnly: false);
	copier.MapPropertyTo<EntityOne, DtoOne>(s => s.ID, t => t.Identity);
	copier.IgnoreProperty<EntityOne, DtoOne>(t => t.Name);
	copier.ForProperty<EntityOne, DtoOne>(t => t.ID, s => s.ID * 2);

Copy to new object:
	
    var entity = new EnitiyOne() { ID = 10 }
    var dto = copier.From(entity).To<DtoOne>();

Copy to existing object:
	
    var dto = new DtoOne { ID = 0 };
    copier.From(new EnitiyOne() { ID = 10 }).To(dto);
	
Copy every object in a collection:

	var list = new List<EntityOne>(setOfItems);
	var result = list.Copy().To<DtoOne>(copier);

# Planned enhancments

* Better API for setting up rules with less repetition
* Support Fields
* Support Internal access for anonymous types