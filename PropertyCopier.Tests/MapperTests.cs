using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace PropertyCopier.Tests
{
    [TestFixture]
    public class MapperTests
    {

        [Test]
        public void CopyNumber()
        {
            var mapper = new Mapper();
            var dto = mapper.Map<EntityOne, DtoOne>(new EntityOne {ID = 10});
            AreEqual(10, dto.ID);
        }

        [Test]
        public void CopyNumberAndString()
        {
            var mapper = new Mapper();
            var dto = mapper.Map<EntityOne, DtoOne>(new EntityOne {ID = 10, Name = "Test"});
            AreEqual(10, dto.ID);
            AreEqual("Test", dto.Name);
        }

        [Test]
        public void CopyStringIgnoreNumber()
        {
            var mapper = new Mapper();
            mapper.IgnoreProperty<EntityOne, DtoOne>(e => e.ID);
            var dto = mapper.Map<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });
            AreEqual(0, dto.ID);
            AreEqual("Test", dto.Name);
        }

        [Test]
        public void CopyStringCustomNumber()
        {
            var mapper = new Mapper();
            mapper.ForProperty<EntityOne, DtoOne, int>(t => t.ID, s => s.ID * 2);
            var dto = mapper.Map<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });
            AreEqual(20, dto.ID);
            AreEqual("Test", dto.Name);
        }

        [Test]
        public void AfterCopyOneAction()
        {
            var mapper = new Mapper();
            mapper.AfterCopy<EntityOne, DtoOne>((e, d) => d.ID = 100);
            var dto = mapper.Map<EntityOne, DtoOne>(new EntityOne {ID = 10, Name = "Test"});
            AreEqual(100, dto.ID);
            AreEqual("Test", dto.Name);
        }

        [Test]
        public void AfterCopyTwoActions()
        {
            var mapper = new Mapper();
            mapper.AfterCopy<EntityOne, DtoOne>((e, d) => d.ID = 100);
            mapper.AfterCopy<EntityOne, DtoOne>((e, d) => d.Name = "100");
            var dto = mapper.Map<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });
            AreEqual(100, dto.ID);
            AreEqual("100", dto.Name);
        }

        [Test]
        public void TwoMappersUseDifferentRulesIgnore()
        {
            var mapper1 = new Mapper();
            var mapper2 = new Mapper();

            mapper1.IgnoreProperty<EntityOne, DtoOne>(t => t.ID);
            mapper2.IgnoreProperty<EntityOne, DtoOne>(t => t.Name);

            var dto1 = mapper1.Map<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });
            var dto2 = mapper2.Map<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });

            AreEqual(0, dto1.ID);
            AreEqual("Test", dto1.Name);

            AreEqual(10, dto2.ID);
            AreEqual(null, dto2.Name);
        }

        [Test]
        public void TwoMappersUseDifferentRulesForProperty()
        {
            var mapper1 = new Mapper();
            var mapper2 = new Mapper();

            mapper1.ForProperty<EntityOne, DtoOne, int>(t => t.ID, s => 100);
            mapper2.ForProperty<EntityOne, DtoOne, string>(t => t.Name, s => "100");

            var dto1 = mapper1.Map<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });
            var dto2 = mapper2.Map<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });

            AreEqual(100, dto1.ID);
            AreEqual("Test", dto1.Name);

            AreEqual(10, dto2.ID);
            AreEqual("100", dto2.Name);
        }

        [Test]
        public void TwoMappersUseDifferentRulesForAfterCopy()
        {
            var mapper1 = new Mapper();
            var mapper2 = new Mapper();

            mapper1.AfterCopy<EntityOne, DtoOne>((s, t) => t.ID = 100);
            mapper2.AfterCopy<EntityOne, DtoOne>((s, t) => t.Name = "100");

            var dto1 = mapper1.Map<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });
            var dto2 = mapper2.Map<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });

            AreEqual(100, dto1.ID);
            AreEqual("Test", dto1.Name);

            AreEqual(10, dto2.ID);
            AreEqual("100", dto2.Name);
        }

        [Test]
        public void MappingInQuery()
        {            
            var sourceData = Builder<EntityOne>.CreateListOfSize(10).Build().AsQueryable();
            var mapper = new Mapper();
            var results = sourceData.Select(mapper.Expression<EntityOne, DtoOne>()).ToList();

            AreEqual(10, results.Count);

            var first = results.First();
            AreEqual(1, first.ID);
            AreEqual("Name1", first.Name);

            var last = results.Last();
            AreEqual(10, last.ID);
            AreEqual("Name10", last.Name);
        }

        [Test]
        public void MappingInQueryWithIgnore()
        {
            var sourceData = Builder<EntityOne>.CreateListOfSize(10).Build().AsQueryable();
            var mapper = new Mapper();
            mapper.IgnoreProperty<EntityOne, DtoOne>(t => t.ID);
            var results = sourceData.Select(mapper.Expression<EntityOne, DtoOne>()).ToList();

            AreEqual(10, results.Count);

            var first = results.First();
            AreEqual(0, first.ID);
            AreEqual("Name1", first.Name);

            var last = results.Last();
            AreEqual(0, last.ID);
            AreEqual("Name10", last.Name);
        }

        [Test]
        public void MappingInQueryWithForProperty()
        {
            var sourceData = Builder<EntityOne>.CreateListOfSize(10).Build().AsQueryable();
            var mapper = new Mapper();
            mapper.ForProperty<EntityOne, DtoOne, int>(t => t.ID, s => s.ID * 2);
            var results = sourceData.Select(mapper.Expression<EntityOne, DtoOne>()).ToList();

            AreEqual(10, results.Count);

            var first = results.First();
            AreEqual(2, first.ID);
            AreEqual("Name1", first.Name);

            var last = results.Last();
            AreEqual(20, last.ID);
            AreEqual("Name10", last.Name);
        }

        [Test]
        public void PropertyDefinedMapping()
        {
            var mapper = new Mapper();
            mapper.SetMappingRule<DateTime, long>(s => s.Ticks);
            var date = new DateTime(1980, 1, 1);

            var dto = mapper.Map<EntityDateTime, DtoDateTicks>(new EntityDateTime {Id = 10, Name = "Type map test", Time = date});
            AreEqual(10, dto.Id);
            AreEqual("Type map test", dto.Name);
            AreEqual(date.Ticks, dto.Time);
        }
    }

    public class EntityDateTime
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime Time { get; set; }
    }

    public class DtoDateTicks
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public long Time { get; set; }
    }
}

