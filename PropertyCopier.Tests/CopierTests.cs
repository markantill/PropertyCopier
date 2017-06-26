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
        [Test(Description = "Copy property Byte to Int32", TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyNumber(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, DtoOne>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var dto = copier.From(new EntityOne {ID = 10}).To<DtoOne>();
            AreEqual(10, dto.ID);
        }

        [Test(Description = "Copy property Byte to Int32 and String to String", TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyNumberAndString(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, DtoOne>(

                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var dto = copier.From(new EntityOne {ID = 10, Name = "Test"}).To<DtoOne>();
            AreEqual(10, dto.ID);
            AreEqual("Test", dto.Name);
        }

        [Test(Description = "Copy one property, ignore one property", TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyStringIgnoreNumber(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, DtoOne>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            copier.SetRules<EntityOne, DtoOne>().IgnoreProperty(e => e.ID);
            var dto = copier.Copy<EntityOne, DtoOne>(new EntityOne {ID = 10, Name = "Test"});
            AreEqual(0, dto.ID);
            AreEqual("Test", dto.Name);
        }

        [Test(Description = "Copy one property, custom rule one property", TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyStringCustomNumber(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, DtoOne>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            copier.SetRules<EntityOne, DtoOne>().ForProperty(t => t.ID, s => s.ID * 2);
            var dto = copier.Copy<EntityOne, DtoOne>(new EntityOne {ID = 10, Name = "Test"});
            AreEqual(20, dto.ID);
            AreEqual("Test", dto.Name);
        }

        [Test(Description = "Copy two properties, one custom action", TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void AfterCopyOneAction(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, DtoOne>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            copier.SetRules<EntityOne, DtoOne>().AfterCopy((e, d) => d.ID = 100);
            var dto = copier.Copy<EntityOne, DtoOne>(new EntityOne {ID = 10, Name = "Test"});
            AreEqual(100, dto.ID);
            AreEqual("Test", dto.Name);
        }

        [Test(Description = "Copy two properties, one custom action", TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void AfterCopyTwoActions(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, DtoOne>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            copier.SetRules<EntityOne, DtoOne>().AfterCopy((e, d) => d.ID = 100);
            copier.SetRules<EntityOne, DtoOne>().AfterCopy((e, d) => d.Name = "100");
            var dto = copier.Copy<EntityOne, DtoOne>(new EntityOne {ID = 10, Name = "Test"});
            AreEqual(100, dto.ID);
            AreEqual("100", dto.Name);
        }

        [Test(Description = "Two copiers. different ignore rules", TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void TwoCopiersUseDifferentRulesIgnore(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier1 = new Copier();
            copier1.SetRules<EntityOne, DtoOne>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var copier2 = new Copier();
            copier2.SetRules<EntityOne, DtoOne>(

                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            copier1.SetRules<EntityOne, DtoOne>().IgnoreProperty(t => t.ID);
            copier2.SetRules<EntityOne, DtoOne>().IgnoreProperty(t => t.Name);

            var dto1 = copier1.Copy<EntityOne, DtoOne>(new EntityOne {ID = 10, Name = "Test"});
            var dto2 = copier2.Copy<EntityOne, DtoOne>(new EntityOne {ID = 10, Name = "Test"});

            AreEqual(0, dto1.ID);
            AreEqual("Test", dto1.Name);

            AreEqual(10, dto2.ID);
            AreEqual(null, dto2.Name);
        }

        [Test(Description = "Two copiers. different custom rules", TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void TwocopiersUseDifferentRulesForProperty(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier1 = new Copier();
            copier1.SetRules<EntityOne, DtoOne>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var copier2 = new Copier();
            copier2.SetRules<EntityOne, DtoOne>(

                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            copier1.SetRules<EntityOne, DtoOne>().ForProperty(t => t.ID, s => 100);
            copier2.SetRules<EntityOne, DtoOne>().ForProperty(t => t.Name, s => "100");

            var dto1 = copier1.Copy<EntityOne, DtoOne>(new EntityOne {ID = 10, Name = "Test"});
            var dto2 = copier2.Copy<EntityOne, DtoOne>(new EntityOne {ID = 10, Name = "Test"});

            AreEqual(100, dto1.ID);
            AreEqual("Test", dto1.Name);

            AreEqual(10, dto2.ID);
            AreEqual("100", dto2.Name);
        }

        [Test(Description = "Two copiers. different after actions", TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void TwocopiersUseDifferentRulesForAfterCopy(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier1 = new Copier();
            copier1.SetRules<EntityOne, DtoOne>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var copier2 = new Copier();
            copier2.SetRules<EntityOne, DtoOne>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            copier1.SetRules<EntityOne, DtoOne>().AfterCopy((s, t) => t.ID = 100);
            copier2.SetRules<EntityOne, DtoOne>().AfterCopy((s, t) => t.Name = "100");

            var dto1 = copier1.Copy<EntityOne, DtoOne>(new EntityOne {ID = 10, Name = "Test"});
            var dto2 = copier2.Copy<EntityOne, DtoOne>(new EntityOne {ID = 10, Name = "Test"});

            AreEqual(100, dto1.ID);
            AreEqual("Test", dto1.Name);

            AreEqual(10, dto2.ID);
            AreEqual("100", dto2.Name);
        }

        [Test(Description = "Expression works in query", TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void MappingInQuery(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var sourceData = Builder<EntityOne>.CreateListOfSize(10).Build().AsQueryable();
            var copier = new Copier();
            copier.SetRules<EntityOne, DtoOne>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var results = sourceData.Select(copier.CopyExpression<EntityOne, DtoOne>()).ToList();

            AreEqual(10, results.Count);

            var first = results.First();
            AreEqual(1, first.ID);
            AreEqual("Name1", first.Name);

            var last = results.Last();
            AreEqual(10, last.ID);
            AreEqual("Name10", last.Name);
        }

        [Test(Description = "Expression with ignore works in query", TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void MappingInQueryWithIgnore(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var sourceData = Builder<EntityOne>.CreateListOfSize(10).Build().AsQueryable();
            var copier = new Copier();
            copier.SetRules<EntityOne, DtoOne>(
                    flattenChildObjects,
                    copyChildObjects,
                    copyChildEnumerations,
                    copyChildCollections,
                    addNullChecking)
                .IgnoreProperty(t => t.ID);

            var results = sourceData.Select(copier.CopyExpression<EntityOne, DtoOne>()).ToList();

            AreEqual(10, results.Count);

            var first = results.First();
            AreEqual(0, first.ID);
            AreEqual("Name1", first.Name);

            var last = results.Last();
            AreEqual(0, last.ID);
            AreEqual("Name10", last.Name);
        }

        [Test(Description = "Expression with custom mapping works in query", TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void MappingInQueryWithForProperty(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var sourceData = Builder<EntityOne>.CreateListOfSize(10).Build().AsQueryable();
            var copier = new Copier();
            copier.SetRules<EntityOne, DtoOne>(
                    flattenChildObjects,
                    copyChildObjects,
                    copyChildEnumerations,
                    copyChildCollections,
                    addNullChecking)
                .ForProperty(t => t.ID, s => s.ID * 2);

            var results = sourceData.Select(copier.CopyExpression<EntityOne, DtoOne>()).ToList();

            AreEqual(10, results.Count);

            var first = results.First();
            AreEqual(2, first.ID);
            AreEqual("Name1", first.Name);

            var last = results.Last();
            AreEqual(20, last.ID);
            AreEqual("Name10", last.Name);
        }

        [Test(Description = "Copy uses custom rules for child properties", TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void PropertyDefinedMapping(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityDateTime, DtoDateTicks>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            copier.SetRules<DateTime, long>().SetMappingRule(s => s.Ticks);
            var date = new DateTime(1980, 1, 1);

            var dto =
                copier.Copy<EntityDateTime, DtoDateTicks>(
                    new EntityDateTime {Id = 10, Name = "Type map test", Time = date});
            AreEqual(10, dto.Id);
            AreEqual("Type map test", dto.Name);
            AreEqual(date.Ticks, dto.Time);
        }

        [Test(Description = "Copy works with target stuct", TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void PropertiesCopiedToStuct(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityDateTime, DtoOneStruct>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var dto =
                copier.Copy<EntityDateTime, DtoOneStruct>(
                    new EntityDateTime {Id = 10, Name = "Test", Time = new DateTime(1980, 1, 1)});
            AreEqual(10, dto.Id);
            AreEqual("Test", dto.Name);
            AreEqual(new DateTime(1980, 1, 1), dto.Time);
        }

        [Test(Description = "Copy list property to target list property with same item types", TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyListToListSameItemType(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityChildListOne, EntityChildListTwo>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var entity = new EntityChildListOne();
            entity.Children = Builder<EntityOne>.CreateListOfSize(10).Build().ToList();
            var dto = copier.Copy<EntityChildListOne, EntityChildListTwo>(entity);

            if (copyChildCollections)
            {
                var zip = dto.Children.Zip(entity.Children, (x, y) => new {FromDto = x, FromEntity = y});
                foreach (var item in zip)
                {
                    AreEqual(item.FromEntity.ID, item.FromDto.ID);
                    AreEqual(item.FromEntity.Name, item.FromDto.Name);
                }
            }
            else
            {
                IsNull(dto.Children);
            }

        }

        [Test(Description = "Copy list property to target list property with different item types",
            TestOf = typeof(Copier))]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyListToListDifferentItemType(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityChildListOne, EntityChildListThree>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var entity = new EntityChildListOne();
            entity.Children = Builder<EntityOne>.CreateListOfSize(10).Build().ToList();
            var dto = copier.Copy<EntityChildListOne, EntityChildListThree>(entity);

            if (copyChildCollections)
            {
                var zip = dto.Children.Zip(entity.Children, (x, y) => new {FromDto = x, FromEntity = y});
                foreach (var item in zip)
                {
                    AreEqual(item.FromEntity.ID, item.FromDto.ID);
                    AreEqual(item.FromEntity.Name, item.FromDto.Name);
                }
            }
            else
            {
                IsNull(dto.Children);
            }
        }

        [Test]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyListToICollectionDifferentItemType(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityChildListOne, EntityChildICollection>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var entity = new EntityChildListOne();
            entity.Children = Builder<EntityOne>.CreateListOfSize(10).Build().ToList();
            var dto = copier.Copy<EntityChildListOne, EntityChildICollection>(entity);

            if (copyChildCollections)
            {
                var zip = dto.Children.Zip(entity.Children, (x, y) => new {FromDto = x, FromEntity = y});
                foreach (var item in zip)
                {
                    AreEqual(item.FromEntity.ID, item.FromDto.ID);
                    AreEqual(item.FromEntity.Name, item.FromDto.Name);
                }
            }
            else
            {
                IsNull(dto.Children);
            }
        }

        [Test]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyListToLinkedListDifferentItemType(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityChildListOne, EntityChildLinkedList>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var entity = new EntityChildListOne();
            entity.Children = Builder<EntityOne>.CreateListOfSize(10).Build().ToList();
            var dto = copier.Copy<EntityChildListOne, EntityChildLinkedList>(entity);

            if (copyChildCollections)
            {
                var zip = dto.Children.Zip(entity.Children, (x, y) => new {FromDto = x, FromEntity = y});
                foreach (var item in zip)
                {
                    AreEqual(item.FromEntity.ID, item.FromDto.ID);
                    AreEqual(item.FromEntity.Name, item.FromDto.Name);
                }
            }
            else
            {
                IsNull(dto.Children);
            }
        }

        [Test]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyListToReadOnlyCollectionDifferentItemType(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityChildListOne, EntityChildReadOnlyCollection>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var entity = new EntityChildListOne();
            entity.Children = Builder<EntityOne>.CreateListOfSize(10).Build().ToList();
            var dto = copier.From(entity).To<EntityChildReadOnlyCollection>();

            if (copyChildCollections)
            {
                var zip = dto.Children.Zip(entity.Children, (x, y) => new {FromDto = x, FromEntity = y});
                foreach (var item in zip)
                {
                    AreEqual(item.FromEntity.ID, item.FromDto.ID);
                    AreEqual(item.FromEntity.Name, item.FromDto.Name);
                }
            }
            else
            {
                IsNull(dto.Children);
            }
        }

#if NET45

        [Test]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyListToIReadOnlyCollectionDifferentItemType(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityChildListOne, EntityChildIReadOnlyCollection>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var entity = new EntityChildListOne();
            entity.Children = Builder<EntityOne>.CreateListOfSize(10).Build().ToList();
            var dto = copier.From(entity).To<EntityChildIReadOnlyCollection>();

            if (copyChildCollections)
            {
                var zip = dto.Children.Zip(entity.Children, (x, y) => new {FromDto = x, FromEntity = y});
                foreach (var item in zip)
                {
                    AreEqual(item.FromEntity.ID, item.FromDto.ID);
                    AreEqual(item.FromEntity.Name, item.FromDto.Name);
                }
            }
            else
            {
                IsNull(dto.Children);
            }
        }

#endif

        [Test]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyListToISetDifferentItemType(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityChildListOne, EntityChildISet>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var entity = new EntityChildListOne();
            entity.Children = Builder<EntityOne>.CreateListOfSize(10).Build().ToList();
            var dto = copier.Copy<EntityChildListOne, EntityChildISet>(entity);

            if (copyChildCollections)
            {
                var zip = dto.Children.Zip(entity.Children, (x, y) => new {FromDto = x, FromEntity = y});
                foreach (var item in zip)
                {
                    AreEqual(item.FromEntity.ID, item.FromDto.ID);
                    AreEqual(item.FromEntity.Name, item.FromDto.Name);
                }
            }
            else
            {
                IsNull(dto.Children);
            }
        }

        [Test]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyWithAssignedNames(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();

            copier.SetRules<EntityOne, EntityDifferentNames>(
                    flattenChildObjects,
                    copyChildObjects,
                    copyChildEnumerations,
                    copyChildCollections,
                    addNullChecking)
                .MapPropertyTo(s => s.ID, t => t.Identity)
                .MapPropertyTo(s => s.Name, t => t.Description);

            var result = copier.From(new EntityOne {ID = 50, Name = "Test"}).To<EntityDifferentNames>();
            AreEqual(50, result.Identity);
            AreEqual("Test", result.Description);
            AreEqual("Test", result.Name);
        }

        [Test]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyWithAssignedNamesAndIgnore(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();

            copier.SetRules<EntityOne, EntityDifferentNames>(
                    flattenChildObjects,
                    copyChildObjects,
                    copyChildEnumerations,
                    copyChildCollections,
                    addNullChecking)
                .MapPropertyTo(s => s.ID, t => t.Identity)
                .MapPropertyTo(s => s.Name, t => t.Description)
                .IgnoreProperty(t => t.Name);

            var result = copier.From(new EntityOne {ID = 50, Name = "Test"}).To<EntityDifferentNames>();
            AreEqual(50, result.Identity);
            AreEqual("Test", result.Description);
            IsNull(result.Name);
        }

        [Test]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyWithNamesDiffernetCaseEnabled(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, EntityDifferentCase>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking,
                StringComparer.InvariantCultureIgnoreCase);

            var result = copier.From(new EntityOne {ID = 10, Name = "Test"}).To<EntityDifferentCase>();
            AreEqual(10, result.id);
            AreEqual("Test", result.name);
        }


        [Test]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyWithNamesDiffernetCaseDisabled(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, EntityDifferentCase>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking,
                comparer: StringComparer.InvariantCulture);

            var result = copier.From(new EntityOne {ID = 10, Name = "Test"}).To<EntityDifferentCase>();
            AreEqual(0, result.id);
            AreEqual(null, result.name);
        }

        [Test]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(true, true, true, true, false, Description = "All other options true")]        
        public void CopyNumberAndStringNullChildPropertiesThrowsException(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, EnitiyTwo>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            Throws<NullReferenceException>(() =>
            {
                copier.From(new EnitiyTwo {ID = 10, Name = "Test", Child = null}).To<DtoTwo>();
            });
        }

        [Test]
        [TestCase(false, false, false, false, true, Description = "All other options false")]
        [TestCase(true, false, false, false, true, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, true, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, true, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, true, Description = "copyChildCollections")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyNumberAndStringAndChildPropertiesNullChecking(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EntityOne, EnitiyTwo>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            copier.SetRules<EnitiyTwo, DtoTwo>(addNullChecking: true);
            var dto =
                copier.From(
                        new EnitiyTwo {ID = 10, Name = "Test", Child = new ChildEntityOne {ID = 100, Name = "Child"}})
                    .To<DtoTwo>();
            AreEqual(10, dto.ID);
            AreEqual("Test", dto.Name);
            AreEqual(100, dto.ChildID);
            AreEqual("Child", dto.ChildName);
        }

        [Test]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyNumberAndStringFlattenChildProperties(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EnitiyTwo, DtoTwo>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var dto = copier.From(new EnitiyTwo
                {
                    ID = 10,
                    Name = "Test",
                    Child = new ChildEntityOne {ID = 100, Name = "Child"}
                })
                .To<DtoTwo>();

            AreEqual(10, dto.ID);
            AreEqual("Test", dto.Name);

            if (flattenChildObjects)
            {
                AreEqual(100, dto.ChildID);
                AreEqual("Child", dto.ChildName);
            }
            else
            {
                AreEqual(0, dto.ChildID);
                IsNull(dto.ChildName);
            }
        }

        [Test]
        [TestCase(false, false, false, false, false, Description = "All options false")]
        [TestCase(true, false, false, false, false, Description = "flattenChildObjects")]
        [TestCase(false, true, false, false, false, Description = "copyChildObjects")]
        [TestCase(false, false, true, false, false, Description = "copyChildEnumerations")]
        [TestCase(false, false, false, true, false, Description = "copyChildCollections")]
        [TestCase(false, false, false, false, true, Description = "addNullChecking")]
        [TestCase(true, true, true, true, true, Description = "All options true")]
        public void CopyNumberAndStringAndChild(
            bool flattenChildObjects,
            bool copyChildObjects,
            bool copyChildEnumerations,
            bool copyChildCollections,
            bool addNullChecking)
        {
            var copier = new Copier();
            copier.SetRules<EnitiyTwo, DtoThree>(
                flattenChildObjects,
                copyChildObjects,
                copyChildEnumerations,
                copyChildCollections,
                addNullChecking);

            var dto =
                copier.From(
                        new EnitiyTwo {ID = 10, Name = "Test", Child = new ChildEntityOne {ID = 100, Name = "Child"}})
                    .To<DtoThree>();
            AreEqual(10, dto.ID);
            AreEqual("Test", dto.Name);

            if (copyChildObjects)
            {
                AreEqual(100, dto.Child.ID);
                AreEqual("Child", dto.Child.Name);
            }
            else
            {
                IsNull(dto.Child);
            }
        }
    }

    #region Test Classes

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

    #endregion Test Classes
}

