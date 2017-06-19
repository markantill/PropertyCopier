using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace PropertyCopier.Tests
{
    [TestFixture]
    public class CopierTests
    {
        [Test]
        public void CopyNumber()
        {
            var copier = new Copier();
            var dto = copier.From(new EntityOne {ID = 10}).To<DtoOne>();
            AreEqual(10, dto.ID);
        }

        [Test]
        public void CopyNumberAndString()
        {
            var copier = new Copier();
            var dto = copier.From(new EntityOne {ID = 10, Name = "Test"}).To<DtoOne>();
            AreEqual(10, dto.ID);
            AreEqual("Test", dto.Name);
        }

        [Test]
        public void CopyStringIgnoreNumber()
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, DtoOne>().IgnoreProperty(e => e.ID);
            var dto = copier.Copy<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });
            AreEqual(0, dto.ID);
            AreEqual("Test", dto.Name);
        }

        [Test]
        public void CopyStringCustomNumber()
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, DtoOne>().ForProperty(t => t.ID, s => s.ID * 2);
            var dto = copier.Copy<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });
            AreEqual(20, dto.ID);
            AreEqual("Test", dto.Name);
        }

        [Test]
        public void AfterCopyOneAction()
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, DtoOne>().AfterCopy((e, d) => d.ID = 100);
            var dto = copier.Copy<EntityOne, DtoOne>(new EntityOne {ID = 10, Name = "Test"});
            AreEqual(100, dto.ID);
            AreEqual("Test", dto.Name);
        }

        [Test]
        public void AfterCopyTwoActions()
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, DtoOne>().AfterCopy((e, d) => d.ID = 100);
            copier.SetRules<EntityOne, DtoOne>().AfterCopy((e, d) => d.Name = "100");
            var dto = copier.Copy<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });
            AreEqual(100, dto.ID);
            AreEqual("100", dto.Name);
        }

        [Test]
        public void TwocopiersUseDifferentRulesIgnore()
        {
            var copier1 = new Copier();
            var copier2 = new Copier();

            copier1.SetRules<EntityOne, DtoOne>().IgnoreProperty(t => t.ID);
            copier2.SetRules<EntityOne, DtoOne>().IgnoreProperty(t => t.Name);

            var dto1 = copier1.Copy<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });
            var dto2 = copier2.Copy<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });

            AreEqual(0, dto1.ID);
            AreEqual("Test", dto1.Name);

            AreEqual(10, dto2.ID);
            AreEqual(null, dto2.Name);
        }

        [Test]
        public void TwocopiersUseDifferentRulesForProperty()
        {
            var copier1 = new Copier();
            var copier2 = new Copier();

            copier1.SetRules<EntityOne, DtoOne>().ForProperty(t => t.ID, s => 100);
            copier2.SetRules<EntityOne, DtoOne>().ForProperty(t => t.Name, s => "100");

            var dto1 = copier1.Copy<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });
            var dto2 = copier2.Copy<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });

            AreEqual(100, dto1.ID);
            AreEqual("Test", dto1.Name);

            AreEqual(10, dto2.ID);
            AreEqual("100", dto2.Name);
        }

        [Test]
        public void TwocopiersUseDifferentRulesForAfterCopy()
        {
            var copier1 = new Copier();
            var copier2 = new Copier();

            copier1.SetRules<EntityOne, DtoOne>().AfterCopy((s, t) => t.ID = 100);
            copier2.SetRules<EntityOne, DtoOne>().AfterCopy((s, t) => t.Name = "100");

            var dto1 = copier1.Copy<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });
            var dto2 = copier2.Copy<EntityOne, DtoOne>(new EntityOne { ID = 10, Name = "Test" });

            AreEqual(100, dto1.ID);
            AreEqual("Test", dto1.Name);

            AreEqual(10, dto2.ID);
            AreEqual("100", dto2.Name);
        }

        [Test]
        public void MappingInQuery()
        {            
            var sourceData = Builder<EntityOne>.CreateListOfSize(10).Build().AsQueryable();
            var copier = new Copier();
            var results = sourceData.Select(copier.CopyExpression<EntityOne, DtoOne>()).ToList();

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
            var copier = new Copier();
            copier.SetRules<EntityOne, DtoOne>().IgnoreProperty(t => t.ID);
            var results = sourceData.Select(copier.CopyExpression<EntityOne, DtoOne>()).ToList();

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
            var copier = new Copier();
            copier.SetRules<EntityOne, DtoOne>().ForProperty(t => t.ID, s => s.ID * 2);
            var results = sourceData.Select(copier.CopyExpression<EntityOne, DtoOne>()).ToList();

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
            var copier = new Copier();
            copier.SetRules<DateTime, long>().SetMappingRule(s => s.Ticks);
            var date = new DateTime(1980, 1, 1);

            var dto = copier.Copy<EntityDateTime, DtoDateTicks>(new EntityDateTime {Id = 10, Name = "Type map test", Time = date});
            AreEqual(10, dto.Id);
            AreEqual("Type map test", dto.Name);
            AreEqual(date.Ticks, dto.Time);
        }

        [Test]
        public void PropertiesCopiedToStuct()
        {
            var copier = new Copier();
            var dto =
                copier.Copy<EntityDateTime, DtoOneStruct>(
                    new EntityDateTime {Id = 10, Name = "Test", Time = new DateTime(1980, 1, 1)});
            AreEqual(10, dto.Id);
            AreEqual("Test", dto.Name);
            AreEqual(new DateTime(1980, 1, 1), dto.Time);
        }

        [Test]
        public void CopyListToListSameItemType()
        {
            var copier = new Copier();
            var entity = new EntityChildListOne();
            entity.Children = Builder<EntityOne>.CreateListOfSize(10).Build().ToList();
            var dto = copier.Copy<EntityChildListOne, EntityChildListTwo>(entity);

            var zip = dto.Children.Zip(entity.Children, (x, y) => new { FromDto = x, FromEntity = y });
            foreach (var item in zip)
            {
                AreEqual(item.FromEntity.ID, item.FromDto.ID);
                AreEqual(item.FromEntity.Name, item.FromDto.Name);
            }
        }

        [Test]
        public void CopyListToListDifferentItemType()
        {
            var copier = new Copier();
            var entity = new EntityChildListOne();
            entity.Children = Builder<EntityOne>.CreateListOfSize(10).Build().ToList();
            var dto = copier.Copy<EntityChildListOne, EntityChildListThree>(entity);

            var zip = dto.Children.Zip(entity.Children, (x, y) => new { FromDto = x, FromEntity = y });
            foreach (var item in zip)
            {
                AreEqual(item.FromEntity.ID, item.FromDto.ID);
                AreEqual(item.FromEntity.Name, item.FromDto.Name);
            }
        }

        [Test]
        public void CopyListToICollectionDifferentItemType()
        {
            var copier = new Copier();
            var entity = new EntityChildListOne();
            entity.Children = Builder<EntityOne>.CreateListOfSize(10).Build().ToList();
            var dto = copier.Copy<EntityChildListOne, EntityChildICollection>(entity);

            var zip = dto.Children.Zip(entity.Children, (x, y) => new { FromDto = x, FromEntity = y });
            foreach (var item in zip)
            {
                AreEqual(item.FromEntity.ID, item.FromDto.ID);
                AreEqual(item.FromEntity.Name, item.FromDto.Name);
            }
        }

        [Test]
        public void CopyListToLinkedListDifferentItemType()
        {
            var copier = new Copier();
            var entity = new EntityChildListOne();
            entity.Children = Builder<EntityOne>.CreateListOfSize(10).Build().ToList();
            var dto = copier.Copy<EntityChildListOne, EntityChildLinkedList>(entity);

            var zip = dto.Children.Zip(entity.Children, (x, y) => new { FromDto = x, FromEntity = y });
            foreach (var item in zip)
            {
                AreEqual(item.FromEntity.ID, item.FromDto.ID);
                AreEqual(item.FromEntity.Name, item.FromDto.Name);
            }
        }

        [Test]
        public void CopyListToReadOnlyCollectionDifferentItemType()
        {
            var copier = new Copier();
            var entity = new EntityChildListOne();
            entity.Children = Builder<EntityOne>.CreateListOfSize(10).Build().ToList();
            var dto = copier.From(entity).To<EntityChildReadOnlyCollection>();

            var zip = dto.Children.Zip(entity.Children, (x, y) => new { FromDto = x, FromEntity = y });
            foreach (var item in zip)
            {
                AreEqual(item.FromEntity.ID, item.FromDto.ID);
                AreEqual(item.FromEntity.Name, item.FromDto.Name);
            }
        }

#if NET45
        [Test]
        public void CopyListToIReadOnlyCollectionDifferentItemType()
        {
            var copier = new Copier();
            var entity = new EntityChildListOne();
            entity.Children = Builder<EntityOne>.CreateListOfSize(10).Build().ToList();
            var dto = copier.From(entity).To<EntityChildIReadOnlyCollection>();

            var zip = dto.Children.Zip(entity.Children, (x, y) => new { FromDto = x, FromEntity = y });
            foreach (var item in zip)
            {
                AreEqual(item.FromEntity.ID, item.FromDto.ID);
                AreEqual(item.FromEntity.Name, item.FromDto.Name);
            }
        }
#endif  

        [Test]
        public void CopyListToISetDifferentItemType()
        {
            var copier = new Copier();
            var entity = new EntityChildListOne();
            entity.Children = Builder<EntityOne>.CreateListOfSize(10).Build().ToList();
            var dto = copier.Copy<EntityChildListOne, EntityChildISet>(entity);

            var zip = dto.Children.Zip(entity.Children, (x, y) => new { FromDto = x, FromEntity = y });
            foreach (var item in zip)
            {
                AreEqual(item.FromEntity.ID, item.FromDto.ID);
                AreEqual(item.FromEntity.Name, item.FromDto.Name);
            }
        }

        [Test]
        public void CopyWithAssignedNames()
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, EntityDifferentNames>().MapPropertyTo(s => s.ID, t => t.Identity);
            copier.SetRules<EntityOne, EntityDifferentNames>().MapPropertyTo(s => s.Name, t => t.Description);
            var result = copier.From(new EntityOne {ID = 50, Name = "Test"}).To<EntityDifferentNames>();
            AreEqual(50, result.Identity);
            AreEqual("Test", result.Description);
            AreEqual("Test", result.Name);
        }

        [Test]
        public void CopyWithAssignedNamesAndIgnore()
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, EntityDifferentNames>()
                .MapPropertyTo(s => s.ID, t => t.Identity)
                .MapPropertyTo(s => s.Name, t => t.Description)
                .IgnoreProperty(t => t.Name);
            var result = copier.From(new EntityOne { ID = 50, Name = "Test" }).To<EntityDifferentNames>();
            AreEqual(50, result.Identity);
            AreEqual("Test", result.Description);
            IsNull(result.Name);
        }

        [Test]
        public void CopyWithNamesDiffernetCaseEnabled()
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, EntityDifferentCase>(comparer: StringComparer.InvariantCultureIgnoreCase);
            var result = copier.From(new EntityOne {ID = 10, Name = "Test"}).To<EntityDifferentCase>();
            AreEqual(10, result.id);
            AreEqual("Test", result.name);
        }


        [Test]
        public void CopyWithNamesDiffernetCaseDisabled()
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, EntityDifferentCase>(comparer: StringComparer.InvariantCulture);
            var result = copier.From(new EntityOne { ID = 10, Name = "Test" }).To<EntityDifferentCase>();
            AreEqual(0, result.id);
            AreEqual(null, result.name);
        }

        [Test]        
        public void CopyNumberAndStringNullChildPropertiesThrowsException()
        {
            var copier = new Copier();

            Throws<NullReferenceException>(() =>
            {
                copier.From(new EnitiyTwo {ID = 10, Name = "Test", Child = null }).To<DtoTwo>();
            });
        }

        [Test]
        public void CopyNumberAndStringAndChildPropertiesNullChecking()
        {
            var copier = new Copier();
            copier.SetRules<EnitiyTwo, DtoTwo>(addNullChecking: true);
            var dto =
                copier.From(
                        new EnitiyTwo { ID = 10, Name = "Test", Child = new ChildEntityOne { ID = 100, Name = "Child" } })
                    .To<DtoTwo>();
            AreEqual(10, dto.ID);
            AreEqual("Test", dto.Name);
            AreEqual(100, dto.ChildID);
            AreEqual("Child", dto.ChildName);
        }

        [Test]
        public void CopyNumberAndStringIgnoreChildProperties()
        {
            var copier = new Copier();
            copier.SetRules<EnitiyTwo, DtoTwo>(flattenChildObjects: false);
            var dto = copier.From(new EnitiyTwo { ID = 10, Name = "Test", Child = new ChildEntityOne { ID = 100, Name = "Child" } }).To<DtoTwo>();

            AreEqual(10, dto.ID);
            AreEqual("Test", dto.Name);

            AreEqual(0, dto.ChildID);
            IsNull( dto.ChildName);
        }

        [Test]
        public void CopyNumberAndStringAndChild()
        {
            var copier = new Copier();
            var dto =
                copier.From(
                        new EnitiyTwo { ID = 10, Name = "Test", Child = new ChildEntityOne { ID = 100, Name = "Child" } })
                    .To<DtoThree>();
            AreEqual(10, dto.ID);
            AreEqual("Test", dto.Name);
            AreEqual(100, dto.Child.ID);
            AreEqual("Child", dto.Child.Name);
        }

        [Test]
        public void CopyNumberAndStringIgnoreChild()
        {
            var copier = new Copier();
            copier.SetRules<EnitiyTwo, DtoThree>(copyChildObjects: false);
            var dto =
                copier.From(
                        new EnitiyTwo { ID = 10, Name = "Test", Child = new ChildEntityOne { ID = 100, Name = "Child" } })
                    .To<DtoThree>();
            AreEqual(10, dto.ID);
            AreEqual("Test", dto.Name);
            IsNull(dto.Child);
        }
    }

    public class EntityDifferentNames
    {
        public int Identity { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }
    }

    public class EntityDifferentCase
    {
        public int id { get; set; }

        public string name { get; set; }
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

    public struct DtoOneStruct
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime Time { get; set; }
    }

    public class EntityChildListOne
    {
        public int Id { get; set; }

        public List<EntityOne> Children { get; set; }
    }

    public class EntityChildListTwo
    {
        public int Id { get; set; }

        public List<EntityOne> Children { get; set; }
    }

    public class EntityChildListThree
    {
        public int Id { get; set; }

        public List<DtoOne> Children { get; set; }
    }

    public class EntityChildICollection
    {
        public int Id { get; set; }

        public ICollection<DtoOne> Children { get; set; }
    }

    public class EntityChildISet
    {
        public int Id { get; set; }

        public ISet<DtoOne> Children { get; set; }
    }

    public class EntityChildIReadOnlyCollection
    {
        public int Id { get; set; }

        public IReadOnlyCollection<DtoOne> Children { get; set; }
    }

    public class EntityChildLinkedList
    {
        public int Id { get; set; }

        public LinkedList<DtoOne> Children { get; set; }
    }

    public class EntityChildReadOnlyCollection
    {
        public int Id { get; set; }

        public ReadOnlyCollection<DtoOne> Children { get; set; }
    }
}

