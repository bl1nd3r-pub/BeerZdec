using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BeerZdec.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BeerOrder> BeerOrders { get; set; }

    public virtual DbSet<BeerOrderItem> BeerOrderItems { get; set; }

    public virtual DbSet<BeerStyle> BeerStyles { get; set; }

    public virtual DbSet<BrewEqType> BrewEqTypes { get; set; }

    public virtual DbSet<BrewEquipment> BrewEquipments { get; set; }

    public virtual DbSet<BrewIngredient> BrewIngredients { get; set; }

    public virtual DbSet<BrewingBatch> BrewingBatches { get; set; }

    public virtual DbSet<Crude> Crudes { get; set; }

    public virtual DbSet<CrudeSupply> CrudeSupplies { get; set; }

    public virtual DbSet<EmpPosition> EmpPositions { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<GBQualGrade> GBQualGrades { get; set; }

    public virtual DbSet<GBStatus> GBStatuses { get; set; }

    public virtual DbSet<Grain> Grains { get; set; }

    public virtual DbSet<GrainBatch> GrainBatches { get; set; }

    public virtual DbSet<HarvestEvent> HarvestEvents { get; set; }

    public virtual DbSet<ISType> ISTypes { get; set; }

    public virtual DbSet<ISTypeCategory> ISTypeCategories { get; set; }

    public virtual DbSet<MaltBatch> MaltBatches { get; set; }

    public virtual DbSet<MaltEquipType> MaltEquipTypes { get; set; }

    public virtual DbSet<MaltEquipment> MaltEquipments { get; set; }

    public virtual DbSet<MaltProcess> MaltProcesses { get; set; }

    public virtual DbSet<MaltingLine> MaltingLines { get; set; }

    public virtual DbSet<MaltingOrder> MaltingOrders { get; set; }

    public virtual DbSet<PackagingBatch> PackagingBatches { get; set; }

    public virtual DbSet<RetailStore> RetailStores { get; set; }

    public virtual DbSet<SoilTextureClass> SoilTextureClasses { get; set; }

    public virtual DbSet<SoilType> SoilTypes { get; set; }

    public virtual DbSet<SowingPlot> SowingPlots { get; set; }

    public virtual DbSet<StorageCell> StorageCells { get; set; }

    public virtual DbSet<StorageMove> StorageMoves { get; set; }

    public virtual DbSet<StorageToMalting> StorageToMaltings { get; set; }

    public virtual DbSet<SuppliableCrude> SuppliableCrudes { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Variety> Varieties { get; set; }

    public virtual DbSet<WareCell> WareCells { get; set; }

    public virtual DbSet<WholesaleCustomer> WholesaleCustomers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BeerOrder>(entity =>
        {
            entity.HasKey(e => e.BeerOrder_ID).HasName("PK_BeerOrder_ID");

            entity.Property(e => e.BeerOrder_Datetime).HasColumnType("datetime");
            entity.Property(e => e.BeerOrder_Status).HasMaxLength(50);

            entity.HasOne(d => d.BeerOrder_EmployeeNavigation).WithMany(p => p.BeerOrders)
                .HasForeignKey(d => d.BeerOrder_Employee)
                .HasConstraintName("FK_BeerOrder_Employee");

            entity.HasOne(d => d.BeerOrder_SellerNavigation).WithMany(p => p.BeerOrders)
                .HasForeignKey(d => d.BeerOrder_Seller)
                .HasConstraintName("FK_BeerOrder_Seller");
        });

        modelBuilder.Entity<BeerOrderItem>(entity =>
        {
            entity.HasKey(e => e.BeerOrderItem_ID).HasName("PK_BeerOrderItem_ID");

            entity.Property(e => e.BeerOrderItem_PricePerUnit).HasColumnType("money");

            entity.HasOne(d => d.BeerOrderItem_OrderNavigation).WithMany(p => p.BeerOrderItems)
                .HasForeignKey(d => d.BeerOrderItem_Order)
                .HasConstraintName("FK_BeerOrderItem_Order");

            entity.HasOne(d => d.BeerOrderItem_PackBatchNavigation).WithMany(p => p.BeerOrderItems)
                .HasForeignKey(d => d.BeerOrderItem_PackBatch)
                .HasConstraintName("FK_BeerOrderItem_PackBatch");
        });

        modelBuilder.Entity<BeerStyle>(entity =>
        {
            entity.HasKey(e => e.BeerStyle_ID).HasName("PK_BeerStyle_ID");

            entity.Property(e => e.BeerStyle_Description)
                .HasMaxLength(200)
                .HasDefaultValue("(Этот стиль не имеет описания)");
            entity.Property(e => e.BeerStyle_IsActive).HasDefaultValue(false);
            entity.Property(e => e.BeerStyle_Name).HasMaxLength(50);
        });

        modelBuilder.Entity<BrewEqType>(entity =>
        {
            entity.HasKey(e => e.BrewEqType_ID).HasName("PK_BrewEqType_ID");

            entity.Property(e => e.BrewEqType_Name).HasMaxLength(50);
        });

        modelBuilder.Entity<BrewEquipment>(entity =>
        {
            entity.HasKey(e => e.BrewEquipment_ID).HasName("PK_BrewEquipment_ID");

            entity.ToTable("BrewEquipment");

            entity.Property(e => e.BrewEquipment_Code).HasMaxLength(50);
            entity.Property(e => e.BrewEquipment_InstallDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.BrewEquipment_MeasUnit).HasMaxLength(20);
            entity.Property(e => e.BrewEquipment_isActive).HasDefaultValue(false);

            entity.HasOne(d => d.BrewEquipment_TypeNavigation).WithMany(p => p.BrewEquipments)
                .HasForeignKey(d => d.BrewEquipment_Type)
                .HasConstraintName("FK_BrewEquipment_Type");
        });

        modelBuilder.Entity<BrewIngredient>(entity =>
        {
            entity.HasKey(e => e.BrewIngr_ID).HasName("PK_BrewIngr_ID");

            entity.HasOne(d => d.BrewIngr_BrewBatchNavigation).WithMany(p => p.BrewIngredients)
                .HasForeignKey(d => d.BrewIngr_BrewBatch)
                .HasConstraintName("FK_BrewIngr_BrewBatch");

            entity.HasOne(d => d.BrewIngr_CrudeNavigation).WithMany(p => p.BrewIngredients)
                .HasForeignKey(d => d.BrewIngr_Crude)
                .HasConstraintName("FK_BrewIngr_Crude");
        });

        modelBuilder.Entity<BrewingBatch>(entity =>
        {
            entity.HasKey(e => e.BrewBatch_ID).HasName("PK_BrewBatch_ID");

            entity.Property(e => e.BrewBatch_Code).HasMaxLength(50);
            entity.Property(e => e.BrewBatch_Datetime).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.BrewBatch_Status).HasMaxLength(50);

            entity.HasOne(d => d.BrewBatch_BeerStyleNavigation).WithMany(p => p.BrewingBatches)
                .HasForeignKey(d => d.BrewBatch_BeerStyle)
                .HasConstraintName("FK_BrewBatch_BeerStyle");

            entity.HasOne(d => d.BrewBatch_ConditionalTankNavigation).WithMany(p => p.BrewingBatchBrewBatch_ConditionalTankNavigations)
                .HasForeignKey(d => d.BrewBatch_ConditionalTank)
                .HasConstraintName("FK_BrewBatch_ConditionalTank");

            entity.HasOne(d => d.BrewBatch_FermenterNavigation).WithMany(p => p.BrewingBatchBrewBatch_FermenterNavigations)
                .HasForeignKey(d => d.BrewBatch_Fermenter)
                .HasConstraintName("FK_BrewBatch_Fermenter");

            entity.HasOne(d => d.BrewBatch_TechnologistNavigation).WithMany(p => p.BrewingBatches)
                .HasForeignKey(d => d.BrewBatch_Technologist)
                .HasConstraintName("FK_BrewBatch_Technologist");
        });

        modelBuilder.Entity<Crude>(entity =>
        {
            entity.HasKey(e => e.Crude_ID).HasName("PK_Crude_ID");

            entity.ToTable("Crude");

            entity.HasOne(d => d.Crude_MaltBatchNavigation).WithMany(p => p.Crudes)
                .HasForeignKey(d => d.Crude_MaltBatch)
                .HasConstraintName("FK_Crude_MaltBatch");

            entity.HasOne(d => d.Crude_OtherBatchNavigation).WithMany(p => p.Crudes)
                .HasForeignKey(d => d.Crude_OtherBatch)
                .HasConstraintName("FK_Crude_OtherBatch");
        });

        modelBuilder.Entity<CrudeSupply>(entity =>
        {
            entity.HasKey(e => e.CrudeSupply_ID).HasName("PK_CrudeSupply_ID");

            entity.Property(e => e.CrudeSupply_Datetime).HasColumnType("datetime");

            entity.HasOne(d => d.CrudeSupply_CrudeNavigation).WithMany(p => p.CrudeSupplies)
                .HasForeignKey(d => d.CrudeSupply_Crude)
                .HasConstraintName("FK_CrudeSupply_Crude");

            entity.HasOne(d => d.CrudeSupply_SupplierNavigation).WithMany(p => p.CrudeSupplies)
                .HasForeignKey(d => d.CrudeSupply_Supplier)
                .HasConstraintName("FK_CrudeSupply_Supplier");
        });

        modelBuilder.Entity<EmpPosition>(entity =>
        {
            entity.HasKey(e => e.EmpPosition_ID).HasName("PK_EmpPosition_ID");

            entity.Property(e => e.EmpPos_Name).HasMaxLength(50);
            entity.Property(e => e.Emp_BaseSalary).HasColumnType("money");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Emp_ID).HasName("PK_Emp_ID");

            entity.Property(e => e.Emp_Email).HasMaxLength(50);
            entity.Property(e => e.Emp_FirstName).HasMaxLength(50);
            entity.Property(e => e.Emp_INN).HasMaxLength(30);
            entity.Property(e => e.Emp_LastName).HasMaxLength(50);
            entity.Property(e => e.Emp_Passport).HasMaxLength(30);
            entity.Property(e => e.Emp_Phone).HasMaxLength(30);
            entity.Property(e => e.Emp_SecName).HasMaxLength(50);

            entity.HasOne(d => d.Emp_PositionNavigation).WithMany(p => p.Employees)
                .HasForeignKey(d => d.Emp_Position)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Emp_Position");
        });

        modelBuilder.Entity<GBQualGrade>(entity =>
        {
            entity.HasKey(e => e.GBQualGrade_ID).HasName("PK_GBQualGrade_ID");

            entity.Property(e => e.GBQualGrade_Name).HasMaxLength(50);
        });

        modelBuilder.Entity<GBStatus>(entity =>
        {
            entity.HasKey(e => e.GBStatus_ID).HasName("PK_GBStatus_ID");

            entity.Property(e => e.GBStatus_Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Grain>(entity =>
        {
            entity.HasKey(e => e.Grain_ID).HasName("PK_Grain_ID");

            entity.Property(e => e.Grain_NameLatin).HasMaxLength(50);
            entity.Property(e => e.Grain_NameRu).HasMaxLength(50);
        });

        modelBuilder.Entity<GrainBatch>(entity =>
        {
            entity.HasKey(e => e.GB_ID).HasName("PK_GB_ID");

            entity.ToTable("GrainBatch");

            entity.HasOne(d => d.GB_HarvestNavigation).WithMany(p => p.GrainBatches)
                .HasForeignKey(d => d.GB_Harvest)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GB_Harvest");

            entity.HasOne(d => d.GB_QualGradeNavigation).WithMany(p => p.GrainBatches)
                .HasForeignKey(d => d.GB_QualGrade)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GB_QualGrade");

            entity.HasOne(d => d.GB_StatusNavigation).WithMany(p => p.GrainBatches)
                .HasForeignKey(d => d.GB_Status)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GB_Status");
        });

        modelBuilder.Entity<HarvestEvent>(entity =>
        {
            entity.HasKey(e => e.HarvestEvent_ID).HasName("PK_HarvestEvent_ID");

            entity.HasOne(d => d.HarvestEvent_SowPlotNavigation).WithMany(p => p.HarvestEvents)
                .HasForeignKey(d => d.HarvestEvent_SowPlot)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HarvestEvent_SowPlot");
        });

        modelBuilder.Entity<ISType>(entity =>
        {
            entity.HasKey(e => e.ISType_ID).HasName("PK_ISType_ID");

            entity.Property(e => e.ISType_Descr).HasMaxLength(200);
            entity.Property(e => e.ISType_Name).HasMaxLength(50);

            entity.HasOne(d => d.ISType_CategoryNavigation).WithMany(p => p.ISTypes)
                .HasForeignKey(d => d.ISType_Category)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ISType_Category");
        });

        modelBuilder.Entity<ISTypeCategory>(entity =>
        {
            entity.HasKey(e => e.ISTC_ID).HasName("PK_ISTC_ID");

            entity.Property(e => e.ISTC_Descr)
                .HasMaxLength(200)
                .HasDefaultValue("У этого типа нет описания");
            entity.Property(e => e.ISTC_Name).HasMaxLength(50);
        });

        modelBuilder.Entity<MaltBatch>(entity =>
        {
            entity.HasKey(e => e.MaltBatch_ID).HasName("PK_MaltBatch_ID");

            entity.HasOne(d => d.MaltBatch_MaltProcessNavigation).WithMany(p => p.MaltBatches)
                .HasForeignKey(d => d.MaltBatch_MaltProcess)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MaltBatch_MaltProcess");
        });

        modelBuilder.Entity<MaltEquipType>(entity =>
        {
            entity.HasKey(e => e.MaltEquipType_ID).HasName("PK_MaltEquipType_ID");

            entity.Property(e => e.MaltEquipType_Description)
                .HasMaxLength(200)
                .HasDefaultValue("У этого типа нет описания");
            entity.Property(e => e.MaltEquipType_Name).HasMaxLength(50);
        });

        modelBuilder.Entity<MaltEquipment>(entity =>
        {
            entity.HasKey(e => e.MaltEquipment_ID).HasName("PK_MaltEquipment_ID");

            entity.ToTable("MaltEquipment");

            entity.Property(e => e.MaltEquipment_InstallDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.MaltEquipment_IsActive).HasDefaultValue(false);
            entity.Property(e => e.MaltEquipment_Manufacturer).HasMaxLength(50);

            entity.HasOne(d => d.MaltEquipment_MaltingLineNavigation).WithMany(p => p.MaltEquipments)
                .HasForeignKey(d => d.MaltEquipment_MaltingLine)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MaltEquipment_MaltingLine");

            entity.HasOne(d => d.MaltEquipment_TypeNavigation).WithMany(p => p.MaltEquipments)
                .HasForeignKey(d => d.MaltEquipment_Type)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MaltEquipment_Type");
        });

        modelBuilder.Entity<MaltProcess>(entity =>
        {
            entity.HasKey(e => e.MaltProcess_ID).HasName("PK_MaltProcess_ID");

            entity.Property(e => e.MaltProcess_EndTime).HasColumnType("datetime");
            entity.Property(e => e.MaltProcess_StartTime).HasColumnType("datetime");

            entity.HasOne(d => d.MaltProcess_MaltLineNavigation).WithMany(p => p.MaltProcesses)
                .HasForeignKey(d => d.MaltProcess_MaltLine)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MaltProcess_MaltLine");

            entity.HasOne(d => d.MaltProcess_MaltOrderNavigation).WithMany(p => p.MaltProcesses)
                .HasForeignKey(d => d.MaltProcess_MaltOrder)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MaltProcess_MaltOrder");

            entity.HasOne(d => d.MaltProcess_TechnologistNavigation).WithMany(p => p.MaltProcesses)
                .HasForeignKey(d => d.MaltProcess_Technologist)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MaltProcess_Technologist");
        });

        modelBuilder.Entity<MaltingLine>(entity =>
        {
            entity.HasKey(e => e.MaltingLine_ID).HasName("PK_MaltingLine_ID");

            entity.Property(e => e.MaltingLine_CurStatus).HasMaxLength(50);
            entity.Property(e => e.MaltingLine_LocationZone).HasMaxLength(50);
        });

        modelBuilder.Entity<MaltingOrder>(entity =>
        {
            entity.HasKey(e => e.MaltingOrder_ID).HasName("PK_MaltingOrder_ID");

            entity.Property(e => e.MaltingOrder_Status).HasMaxLength(50);
            entity.Property(e => e.MaltingOrder_TargetMaltType).HasMaxLength(50);
            entity.Property(e => e.MaltingOrder_СreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<PackagingBatch>(entity =>
        {
            entity.HasKey(e => e.PackBatch_ID).HasName("PK_PackBatch_ID");

            entity.Property(e => e.PackBatch_Datetime).HasColumnType("datetime");
            entity.Property(e => e.PackBatch_Status).HasMaxLength(50);
            entity.Property(e => e.PackBatch_Type).HasMaxLength(50);

            entity.HasOne(d => d.PackBatch_BrewBatchNavigation).WithMany(p => p.PackagingBatches)
                .HasForeignKey(d => d.PackBatch_BrewBatch)
                .HasConstraintName("FK_PackBatch_BrewBatch");
        });

        modelBuilder.Entity<RetailStore>(entity =>
        {
            entity.HasKey(e => e.RetailStore_ID).HasName("PK_RetailStore_ID");

            entity.Property(e => e.RetailStore_Address).HasMaxLength(50);
            entity.Property(e => e.RetailStore_IsActive).HasDefaultValue(false);
            entity.Property(e => e.RetailStore_Name).HasMaxLength(50);

            entity.HasOne(d => d.RetailStore_ManagerNavigation).WithMany(p => p.RetailStores)
                .HasForeignKey(d => d.RetailStore_Manager)
                .HasConstraintName("FK_RetailStore_Manager");
        });

        modelBuilder.Entity<SoilTextureClass>(entity =>
        {
            entity.HasKey(e => e.SoilTextureClass_ID).HasName("PK_SoilTextureClass_ID");

            entity.Property(e => e.SoilTextureClass_Name).HasMaxLength(50);
        });

        modelBuilder.Entity<SoilType>(entity =>
        {
            entity.HasKey(e => e.SoilType_ID).HasName("PK_SoilType_ID");

            entity.Property(e => e.SoilType_Name).HasMaxLength(50);

            entity.HasOne(d => d.SoilType_TextureClassNavigation).WithMany(p => p.SoilTypes)
                .HasForeignKey(d => d.SoilType_TextureClass)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SoilType_TextureClass");
        });

        modelBuilder.Entity<SowingPlot>(entity =>
        {
            entity.HasKey(e => e.SowingPlot_ID).HasName("PK_SowingPlot_ID");

            entity.HasOne(d => d.SowPlot_IrrigationSystemTypeNavigation).WithMany(p => p.SowingPlots)
                .HasForeignKey(d => d.SowPlot_IrrigationSystemType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SowPlot_IrrigationSystemType");

            entity.HasOne(d => d.SowPlot_SoilTypeNavigation).WithMany(p => p.SowingPlots)
                .HasForeignKey(d => d.SowPlot_SoilType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SowPlot_SoilType");
        });

        modelBuilder.Entity<StorageCell>(entity =>
        {
            entity.HasKey(e => e.Storage_ID).HasName("PK_Storage_ID");
        });

        modelBuilder.Entity<StorageMove>(entity =>
        {
            entity.HasKey(e => e.StorageMoves_Zapis_ID).HasName("PK_StorageMoves_Zapis_ID");

            entity.HasOne(d => d.StorageMoves_FromStorageNavigation).WithMany(p => p.StorageMoveStorageMoves_FromStorageNavigations)
                .HasForeignKey(d => d.StorageMoves_FromStorage)
                .HasConstraintName("FK_StorageMoves_FromStorage");

            entity.HasOne(d => d.StorageMoves_GrainBatchNavigation).WithMany(p => p.StorageMoves)
                .HasForeignKey(d => d.StorageMoves_GrainBatch)
                .HasConstraintName("FK_StorageMoves_GrainBatch");

            entity.HasOne(d => d.StorageMoves_MovedByNavigation).WithMany(p => p.StorageMoves)
                .HasForeignKey(d => d.StorageMoves_MovedBy)
                .HasConstraintName("FK_StorageMoves_MovedBy");

            entity.HasOne(d => d.StorageMoves_ToStorageNavigation).WithMany(p => p.StorageMoveStorageMoves_ToStorageNavigations)
                .HasForeignKey(d => d.StorageMoves_ToStorage)
                .HasConstraintName("FK_StorageMoves_ToStorage");
        });

        modelBuilder.Entity<StorageToMalting>(entity =>
        {
            entity.HasKey(e => e.STM_Zapis_ID).HasName("PK_STM_Zapis_ID");

            entity.ToTable("StorageToMalting");

            entity.Property(e => e.STM_Datetime).HasColumnType("datetime");

            entity.HasOne(d => d.STM_MaltOrderNavigation).WithMany(p => p.StorageToMaltings)
                .HasForeignKey(d => d.STM_MaltOrder)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_STM_MaltOrder");

            entity.HasOne(d => d.STM_StorageNavigation).WithMany(p => p.StorageToMaltings)
                .HasForeignKey(d => d.STM_Storage)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_STM_Storage");
        });

        modelBuilder.Entity<SuppliableCrude>(entity =>
        {
            entity.HasKey(e => e.SuppliableCrude_ID).HasName("PK_SuppliableCrude_ID");

            entity.ToTable("SuppliableCrude");

            entity.Property(e => e.SuppliableCrude_MeasurementUnit).HasMaxLength(20);
            entity.Property(e => e.SuppliableCrude_Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Supplier_ID).HasName("PK_Supplier_ID");

            entity.Property(e => e.Supplier_Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Users_Id");

            entity.HasIndex(e => e.UsLogin, "UQ__Users__C1F9CC7159B3AD87").IsUnique();

            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("User");
            entity.Property(e => e.UsLogin).HasMaxLength(50);
            entity.Property(e => e.UsPassword).HasMaxLength(100);
        });

        modelBuilder.Entity<Variety>(entity =>
        {
            entity.HasKey(e => e.Variety_ID).HasName("PK_Variety_ID");

            entity.Property(e => e.Variety_GosRegNum).HasMaxLength(20);
            entity.Property(e => e.Variety_MaltingPurpose).HasMaxLength(20);
            entity.Property(e => e.Variety_MaturityGroup).HasMaxLength(50);
            entity.Property(e => e.Variety_NameLatin).HasMaxLength(50);
            entity.Property(e => e.Variety_NameRu).HasMaxLength(50);
            entity.Property(e => e.Variety_RowType).HasMaxLength(50);
            entity.Property(e => e.Variety_SeasonType).HasMaxLength(50);

            entity.HasOne(d => d.Variety_GrainNavigation).WithMany(p => p.Varieties)
                .HasForeignKey(d => d.Variety_Grain)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Variety_Grain");
        });

        modelBuilder.Entity<WareCell>(entity =>
        {
            entity.HasKey(e => e.WareCell_ID).HasName("PK_WareCell_ID");

            entity.Property(e => e.WareCell_Condition).HasMaxLength(50);

            entity.HasOne(d => d.WareCell_CrudeNavigation).WithMany(p => p.WareCells)
                .HasForeignKey(d => d.WareCell_Crude)
                .HasConstraintName("FK_WareCell_Crude");
        });

        modelBuilder.Entity<WholesaleCustomer>(entity =>
        {
            entity.HasKey(e => e.Customer_ID).HasName("PK_Customer_ID");

            entity.Property(e => e.Customer_CompanyName).HasMaxLength(50);
            entity.Property(e => e.Customer_ContactPerson).HasMaxLength(50);
            entity.Property(e => e.Customer_DeliveryAddress).HasMaxLength(50);
            entity.Property(e => e.Customer_Email).HasMaxLength(30);
            entity.Property(e => e.Customer_INN).HasMaxLength(20);
            entity.Property(e => e.Customer_IsActive).HasDefaultValue(false);
            entity.Property(e => e.Customer_Phone).HasMaxLength(30);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
