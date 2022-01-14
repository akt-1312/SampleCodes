using HMS.Models.Administration;
using HMS.Models.Doctor;
using HMS.Models.Nurse;
using HMS.Models.Reception;
using HMS.Models.SeedData;
using HMS.Models.SuperAdmin;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.SeedData();

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                    Name = "Admin",
                    NormalizedName = "Admin".ToUpper(),
                    ConcurrencyStamp = "45703f67-49ae-43a8-8b5a-565cdd2eb02b"
                });

            foreach (var foreighKey in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                foreighKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

        }

        //Administration
        public DbSet<Module> Modules { get; set; }
        public DbSet<PageNamesInModule> PageNamesInModules { get; set; }
        public DbSet<PagesInRole> PagesInRoles { get; set; }

        public DbSet<DoctorInfo> DoctorInfos { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<UnitsOfDoctor> UnitsOfDoctors { get; set; }

        public DbSet<HomeLocalForeignerCurrencyType> HomeLocalForeignerCurrencyTypes { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<CurrencyRate> CurrencyRates { get; set; }
        public DbSet<ServiceGroupType> ServiceGroupTypes { get; set; }
        public DbSet<Service> Services { get; set; }

        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Township> Townships { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<IdentityType> IdentityTypes { get; set; }
        public DbSet<Nationality> Nationalities { get; set; }
        public DbSet<Occupation> Occupations { get; set; }
        public DbSet<Prefix> Prefixes { get; set; }
        public DbSet<Relationship> Relationships { get; set; }
        public DbSet<MaritalStatus> MaritalStatuses { get; set; }
        public DbSet<ReferalHospital> ReferalHospitals { get; set; }
        public DbSet<GeneralService> GeneralServices { get; set; }
        public DbSet<RegistrationService> RegistrationServices { get; set; }

        public DbSet<DoctorDuty> DoctorDuties { get; set; }

        public DbSet<AllergicType> AllergicTypes { get; set; }
        public DbSet<OnSet> OnSets { get; set; }
        public DbSet<OnSetType> OnSetTypes { get; set; }
        public DbSet<Reaction> Reactions { get; set; }


        //Reception
        public DbSet<PatientRegistration> PatientRegistrations { get; set; }
        public DbSet<DoctorAppointmentData> DoctorAppointmentDatas { get; set; }
        public DbSet<PatientCase> PatientCases { get; set; }
        public DbSet<PatientServiceFees> PatientServiceFees { get; set; }
        public DbSet<NewPatientBookingAppointment> NewPatientBookingAppointments { get; set; }

        //Nrse
        public DbSet<Allergic> Allergics { get; set; }
        public DbSet<AllergicPatientReaction> AllergicPatientReactions { get; set; }
        public DbSet<UnitOfMeasurement> UnitOfMeasurements { get; set; }
        public DbSet<VitalSignSetup> VitalSignSetups { get; set; }
        public DbSet<PatientVitalInfo> PatientVitalInfos { get; set; }

        //Doctor
        public DbSet<CrossConsultation> CrossConsultations { get; set; }
    }
}
