using FluentAssertions;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using NHibernate;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace FixOracleDatesNotUsingIndex
{
    public class Testing
    {
        public static void RepeatAction(Action action)
        {
            for (int i = 0; i < 10; i++)
                action();
        }

        [Test]
        public void SpeedTest()
        {
            Console.WriteLine("Starting OptimizedEntity");
            using (var sf = NhibernateConfiguration.CreateSessionFactory())
            using (var session = sf.OpenSession())
            {
                var sw = Stopwatch.StartNew();

                RepeatAction(() =>
                {
                    session.QueryOver<OptimizedEntity>()
                        .Where(x => x.SomeDate == DateTime.Now)
                        .List();
                });

                Console.WriteLine("ElapsedMilliseconds: " + sw.ElapsedMilliseconds);
            }

            Console.WriteLine("Starting SlowEntity");
            using (var sf = NhibernateConfiguration.CreateSessionFactory())
            using (var session = sf.OpenSession())
            {
                var sw = Stopwatch.StartNew();

                RepeatAction(() =>
                {
                    session.QueryOver<SlowEntity>()
                        .Where(x => x.SomeDate == DateTime.Now)
                        .List();
                });

                Console.WriteLine("ElapsedMilliseconds: " + sw.ElapsedMilliseconds);
            }
        }

        [Test]
        public void OracleDateType_WillWork()
        {
            using (var sf = NhibernateConfiguration.CreateSessionFactory())
            using (var session = sf.OpenSession())
            {
                var date = new DateTime(2017, 6, 8, 12, 10, 59);
                var entity = new OptimizedEntity { Id = "123456", SomeDate = date };
                session.Save(entity);
                session.Flush();
                session.Clear();

                session.QueryOver<SlowEntity>()
                    .Where(x => x.SomeDate == date)
                    .SingleOrDefault().Id.Should().Be(entity.Id);

                session.Delete(entity);
                session.Flush();
                session.Clear();

                session.QueryOver<SlowEntity>()
                    .Where(x => x.SomeDate == date)
                    .List().Should().BeEmpty();
            }
        }
    }

    public class NhibernateConfiguration
    {
        public static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(OracleDataClientConfiguration
                            .Oracle10
                            .ConnectionString("Data Source=***;User Id=sys;Password=***;DBA Privilege=SYSDBA;")
                            .UseReflectionOptimizer()
                            )
                            .Cache(c => c.UseMinimalPuts())
                    .Mappings(m => { m.FluentMappings.AddFromAssemblyOf<OptimizedMap>(); })
                .BuildSessionFactory();
        }
    }

    public class OptimizedEntity
    {
        public string Id { get; set; }
        public DateTime SomeDate { get; set; }
    }

    public class SlowEntity 
    {
        public string Id { get; set; }
        public DateTime SomeDate { get; set; }
    }

    public class OptimizedMap : ClassMap<OptimizedEntity>
    {
        public OptimizedMap()
        {
            Schema("some schema");
            Table("some table");
            Not.LazyLoad();

            Id(x => x.Id);
            Map(x => x.SomeDate, "updat").CustomType<OracleDateType>();
        }
    }

    public class SlowMap : ClassMap<SlowEntity>
    {
        public SlowMap()
        {
            Schema("some schema");
            Table("some table");
            Not.LazyLoad();

            Id(x => x.Id);
            Map(x => x.SomeDate, "updat");
        }
    }
}