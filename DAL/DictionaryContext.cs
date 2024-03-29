﻿using Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class DictionaryContext : IdentityDbContext<Person>
    {
        public DictionaryContext() : base("DictionaryContext")
        {

        }
        public virtual DbSet<Language> Languages { get; set; }
       // public virtual DbSet<Person> People { get; set; }// identity den geliyor
       public virtual DbSet<Word> Words { get; set; }
       public virtual DbSet<WordRequest> WordRequests { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            #region TableConfiguration
            modelBuilder.Entity<Language>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Word>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<WordRequest>()
                .HasKey(x => x.Id);
            #endregion

            #region Relations
            modelBuilder.Entity<Language>()
                .HasMany(x => x.Words)
                .WithRequired(x => x.Language)
                .HasForeignKey(x => x.Language_Id);

            modelBuilder.Entity<Word>()
                .HasMany(x => x.Translations)
                .WithMany();

            modelBuilder.Entity<WordRequest>()
                .HasRequired(x => x.Person)
                .WithMany(x => x.Requests);

            modelBuilder.Entity<WordRequest>()
                .HasRequired(x => x.Language)
                .WithMany(x => x.Requests)
                .HasForeignKey(x => x.LanguageId);
            #endregion

            base.OnModelCreating(modelBuilder);
        }
    }
}
