
namespace RafaFrameworkCSharp
{
    using Negocio;
    using Negocio.Core;
    using Negocio.Entities;
    using System.Collections.Generic;
    using System.Data;
    using System.Windows.Forms;
    using MessageService = Negocio.Entities.Messenger.Manager;
    public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
      Init();
    }

    public void Init()
    {
      try
      {

        using (Dal.Core.DbContext __dbContext = new Dal.Core.DbContext())
        {
          __dbContext.BeginTransaction();
          // ============================================
          // Serialization
          // ============================================
          TestSerialization(__dbContext);
          // ============================================
          // SmallXmlSerializer
          // ============================================
          TestSmallXmlSerializer(__dbContext);
          // ============================================
          // SqlDirectQuery
          // ============================================
          TestSqlDirectQuery(__dbContext);
          // ============================================
          // SmallJsonSerializer
          // ============================================
          TestSmallJsonSerializer(__dbContext);
          // ============================================
          // Carga, actualización, inserción y borrado
          // ============================================
          TestCrud(__dbContext);
          // ============================================
          // Transacciones
          // ============================================
          __dbContext.Commit();
          __dbContext.Rollback();                          
          // =========================================================
          // Dal.Repositories.DynamicRepository con nombre o prefijo
          // =========================================================
          TestDynamicRepositoryWithPrefix(__dbContext);
          // =========================================================
          // Dal.Repositories.DynamicRepository SIN nombre o prefijo
          // =========================================================
          TestDynamicRepository();
          // =========================================================
          // BulkCopy
          // =========================================================
          TestBulkCopy(__dbContext);
          // =========================================================
          // Messages
          // =========================================================
          TestMessageService(__dbContext);
        }
      }
      catch (System.Exception ex)
      {
        System.Diagnostics.Trace.WriteLine(ex.ToString());
      }

    }
    private void TestMessageService(Dal.Core.DbContext dbContext)
    {
        // =========================================================================
        // Creación de usuarios
        // =========================================================================
        MessageService.SaveUser("04179642J", "Rafael Castro");
        int __idDesarrollo_1 = MessageService.SaveUser("Desa01", "Desarrollo1");
        int __idDesarrollo_2 = MessageService.SaveUser("Desa02", "Desarrollo2");
        // =========================================================================
        // Eliminar usuarios de TODOS los grupos a los que pertenezcan
        // =========================================================================
        MessageService.RemoveMembersFromAllGroups(dbContext, 
                                                  new string[] { "Desa01", 
                                                                 "Desa02" });
        // =========================================================================
        // Creación de grupos y asignación de usuarios
        // =========================================================================
        var __grupo = new Negocio.Entities
                                 .Messenger
                                 .Group(dbContext)
                                 .LoadByName("Desarrollo");
        if(__grupo.Id == 0)
        {
            __grupo.Name = "Desarrollo";
            __grupo.Save();
            __grupo.AddMembers(new int[] { __idDesarrollo_1,
                                           __idDesarrollo_2 });
        }
        // =========================================================================
        // Carga del número de mensajes sin leer
        // =========================================================================
        var __number = MessageService.CountOfUnreadMessages(dbContext, "04179642J");
        // =========================================================================
        // Envío de mensajes
        // =========================================================================
        MessageService.Create(dbContext)
                      .Subject("TestMessageService") 
                      .Message("Hola a todos")
                      .Send("System,@Desarrollo,04179642J,#5");
        MessageService.Create(dbContext)
                      .Subject("TestMessageService") 
                      .Message("Hola desarrollo3")
                      .Send("#5");
    }

    private void TestSerialization(Dal.Core.DbContext dbContext)
    {
      // =====================================================================
      // Carga de datos
      // =====================================================================
      SampleUsuario __usuario = new SampleUsuario(dbContext).Load(6);
      Perfiles __perfiles = new Perfiles(dbContext).Load();
      // =====================================================================
      // ToJsonString()
      // =====================================================================
      string __json = __usuario.ToJsonString();
      // =====================================================================
      // ToXml()
      // =====================================================================
      string __xml = __usuario.ToXml();
      string __clearXml = __usuario.ToClearXml();
      // =====================================================================
      // FromJsonTo()
      // =====================================================================
      SampleUsuario __u1 = __json.FromJsonTo<Negocio.Entities.SampleUsuario>();
      // =====================================================================
      // FromXmlTo()
      // =====================================================================
      SampleUsuario __u2 = __xml.FromXmlTo<Negocio.Entities.SampleUsuario>();
      // ==================================================================================
      // Serialización por el método de extensión
      // ==================================================================================
      __json = __perfiles.ToJsonString(new FieldInfo[]{
                                         new FieldInfo(typeof(int), "Id", "_identificador")
                                       });
    }

    private void TestSmallXmlSerializer(Dal.Core.DbContext dbContext)
    {
      // =====================================================================
      // Carga de datos
      // =====================================================================
      Coordinados __coordinados = new Coordinados(dbContext).Load();
      Perfiles __perfiles = new Perfiles(dbContext).Load();
      Perfil __perfil = new Perfil(dbContext).Load(6);

      string __json =  SerializersStringRepository.GetNamedSerializer(typeof(Perfil), "PerfilSmall").ToJsonString(__perfiles);
      __json =  SerializersStringRepository.GetNamedSerializer(typeof(Perfil), "Perfil").ToAssociativeArrayJsonString(__perfiles);
      __json =  SerializersStringRepository.GetNamedSerializer(typeof(Perfil), "Perfil").ToJsonString(__perfiles);  
      __json =  SerializersStringRepository.GetNamedSerializer(typeof(Perfil), "PerfilSmall").ToJsonString(__perfil);
      __json =  SerializersStringRepository.GetNamedSerializer(typeof(Perfil), "Perfil").ToJsonString(__perfil);      
      // ====================================================================================================================
      // Propiedades personalizadas
      // ====================================================================================================================
      SmallXmlSerializer __serializer = new SmallXmlSerializer(typeof(Perfil),
                                                               FieldInfo.FromString("String,Codigo,__c#String, ~xj5, __d"));
      //__perfiles.SetDataProvider(__data_provider);
      __perfiles.SetDataProvider(delegate (string key, Entity value) {
        if (key == "xj5") return "__xj5" + ((Perfil)value).Descripcion;
        return "";        
      });
      // ===============================================
      // ToJsonString() Elemento y collección
      // ===============================================
      __json = __serializer.ToJsonString(__perfil);
      __json = __serializer.ToJsonString(__perfiles);
      // ===============================================
      // ToXml() Elemento y collección
      // ===============================================
      string __xml = __serializer.ToXmlString(__perfil);
      __xml = __serializer.ToXmlString( __perfiles );
      // ===============================================
      // ToCsv() Elemento y collección
      // ===============================================
      __xml = __serializer.ToCsvString(__perfil);
      __xml = __serializer.ToCsvString( __perfiles );
      // ===============================================
      // ToExcelString() Elemento y collección
      // ===============================================
      __xml = __serializer.ToExcelString(__perfil);
      __xml = __serializer.ToExcelString( __perfiles );

    }

    private void TestSqlDirectQuery(Dal.Core.DbContext dbContext)
    {
      // =================================================================================================================
      // CreateAndFillSerializerFromQuery
      // =================================================================================================================
      var __serializer = SqlDirectQuery.CreateAndFillSerializerFromQuery("SqlDirectQuery_T_SEG_USUARIO",
                                                                          dbContext,
                                                                          @"SELECT CD_USUARIO AS cod, DS_USUARIO as des 
                                                                            FROM T_SEG_USUARIOS");
      var __data = __serializer.GetValues();
      string __json = __serializer.ToJsonString();
      // =================================================================================================================
      // CreateAndFillSerializer - NamedQuery
      // =================================================================================================================
      __serializer = SqlDirectQuery.CreateAndFillSerializer("SqlDirectQuery_T_COORDINADOS",
                                                             dbContext,
                                                             "Dal.Repositories.CoordinadosRepository.Select");
      __data = __serializer.GetValues();
      __json = __serializer.ToJsonString();
      // =================================================================================================================
      // CreateAndFillSerializer - NamedQuery, ExtensionPoint, QueryBuilder
      // =================================================================================================================
      __serializer = SqlDirectQuery.CreateAndFillSerializer("SqlDirectQuery_T_COORDINADOS_2",
                                                             dbContext,
                                                             "Dal.Repositories.CoordinadosRepository.Select",
                                                             delegate (string key, object value) {
                                                                if (key == "key1") return 1;
                                                                return 1;        
                                                                },
                                                             "#int,~key1,indexValue",
                                                             new Dal.Core.QueryBuilder()
                                                                         .UseParam("ID","5")
                                                                         .AndInteger("ID"));
      __data = __serializer.GetValues();
      __json = __serializer.ToJsonString();
    }

    private void TestSmallJsonSerializer(Dal.Core.DbContext dbContext)
    {
      // ===================================================================================================
      // Carga de datos
      // ===================================================================================================
      Perfiles __perfiles = new Perfiles(dbContext).Load();
      // ===================================================================================================
      // Serialización de objetos Entity
      // ===================================================================================================
      SmallJsonSerializer __serializer = new SmallJsonSerializer(typeof(Perfil),
                                                                  new FieldInfo[]{
                                                                      new FieldInfo(typeof(int), "Id", "_i")
                                                                  });
      string __data = __serializer.ToJsonString(__perfiles);
      // ===================================================================================================
      // NamedSerializer
      // ===================================================================================================
      FieldInfo[] __info = FieldInfo.FromString(SerializersStringRepository.ValueFromKey("PerfilXXX"));
      __data = new SmallJsonSerializer( typeof(Perfil), __info ).ToJsonString(__perfiles);
      // ===================================================================================================
      // NamedSerializer #PerfilXXX@Negocio.Entities.Perfil
      // ===================================================================================================
      __data = new Negocio.Core.SmallJsonSerializer( "PerfilXXX", __perfiles).ToJsonString();          
      // ===================================================================================================
      // Serialización de otros tipos de objetos. Propiedades (Id, Name) y campos (tag)
      // ===================================================================================================
      var __datos = new DataContainer[] {
                      new DataContainer(){Id = 5, Name = "Rafa", Tag = "Padre" },
                      new DataContainer(){Id = 15, Name = "María", Tag = "Hija" }
                    };
      __data = new SmallJsonSerializer( typeof(DataContainer),
                                        __datos, 
                                        new[] { (typeof(int), "Id", "_id"),
                                                (typeof(string), "Name", "_name"),
                                                (typeof(string), "Tag", "_tag")})
                                      .ToJsonString();
    }

    private void TestCrud(Dal.Core.DbContext dbContext)
    {
      // ===============================================================
      // Carga
      // ===============================================================
      var __usuarios = new Negocio.Entities.Usuarios(dbContext).Load();
      var __usuario = new Negocio.Entities.Usuario(dbContext).Load(6);
      // ===============================================================
      // Actualización
      // ===============================================================
      __usuario.Save();
      // ===============================================================
      // Inserción
      // ===============================================================
      __usuario.Id = 0;
      __usuario.Codigo = "RAFA-" + System.DateTime.Now.Ticks.ToString();
      __usuario.Save();
      // ===============================================================
      // Borrado
      // ===============================================================
      __usuario.Delete();
    }

    private void TestDynamicRepositoryWithPrefix(Dal.Core.DbContext dbContext)
    {
      using (Dal.Repositories.DynamicRepository __repo = new Dal.Repositories.DynamicRepository(dbContext, "Dal.Repositories.table_name"))
      {
        // ===============================================================================================
        // Uso de ExecuteReader
        // ===============================================================================================
        using (var reader = __repo.ExecuteReader("SELECT ID,CD_USUARIO,DS_USUARIO FROM T_SEG_USUARIOS"))
        {
          List<object[]> rows = new List<object[]>();
          while (reader.Read())
          {
            object[] row = new object[2];
            reader.GetValues(row);
            rows.Add(row);
          }            
        }
        // ===============================================================================================
        // Uso de ExecuteNamedScalar
        // ===============================================================================================     
        int __result = __repo.ExecuteNamedScalar<int>("Count", new string[] { "5" });
        // ===============================================================================================
        // Uso de ExecuteNamedReader
        // ===============================================================================================       
        using (var reader = __repo.ExecuteNamedReader("SelectAll"))
        {
            List<object[]> rows = new List<object[]>();
            while (reader.Read())
            {
            object[] row = new object[6];
            reader.GetValues(row);
            rows.Add(row);
            }
        }
        // ===============================================================================================
        // Carga de entidades desde los valores de una query
        // ===============================================================================================
        Usuarios __usuarios = (Usuarios)__repo.Load(new Usuarios(), 
                                                    __repo.ExecuteReader("SELECT * FROM T_SEG_USUARIOS"));
        // ===============================================================================================
        // Carga de entidades desde los valores de una query con nombre
        // ===============================================================================================
        __usuarios = (Usuarios)__repo.Load(new Usuarios(), "SelectAll");

        // ======================================================================================
        // Carga de una entidad desde los valores de una query
        // ======================================================================================   
        Usuario __usuario = __repo.LoadOne(new Usuario(), 
                                           __repo.ExecuteReader("SELECT * FROM T_SEG_USUARIOS"));
        // ======================================================================================
        // Carga de una entidad desde los valores de una query con nombre
        // ======================================================================================
        __usuario = __repo.LoadOne(new Usuario(), "SelectAll");
        // ======================================================================================
        // Carga de una entidad desde los valores de una query con nombre
        // ======================================================================================
        __usuario = __repo.LoadOne(new Usuario(), __repo.ExecuteNamedReader("SelectAll"));
      }
    }

    private void TestDynamicRepository()
    {
      using (Dal.Repositories.DynamicRepository __repo = new Dal.Repositories.DynamicRepository())
      {
        // ============================================================================================================
        // Cargar un elemento con los valores de una query
        // ============================================================================================================
        Usuario __usuario = __repo.LoadOne(new Usuario(), __repo.ExecuteReader("SELECT * FROM T_SEG_USUARIOS"));
        // ============================================================================================================
        // Cargar un elemento con los valores de una query con nombre
        // ============================================================================================================
        __usuario = __repo.LoadOne(new Usuario(), "Dal.Repositories.table_name.SelectAll");
        // ============================================================================================================
        // Cargar un elemento con los valores de un dataReader
        // ============================================================================================================
        __usuario = __repo.LoadOne(new Usuario(), __repo.ExecuteNamedReader("Dal.Repositories.table_name.SelectAll"));
        // ============================================================================================================
        // Cargar un elemento con los valores de un dataReader con un cargador en concreto
        // ============================================================================================================
        __usuario = __repo.LoadOne(new Usuario(), 
                                   __repo.ExecuteNamedReader("Dal.Repositories.table_name.SelectAll"),
                                   new Dal.Core.Loader.StringBinder("Loader_0001", "0,_id, Integer;2,_descripcion"));
        // ============================================================================================================
        // Cargar un elemento utilizando ParameterContainer y QueryBuilder y tuplas
        // ============================================================================================================
        var builder = new Dal.Core.ParameterContainer()
                                  .Add("ID", "5")
                                  .AddRange(new[] { ("Monroe", "1"),
                                                  ("CD_USUARIO", "US_PARTICIPANTE AVANZADO"),
                                                  ("New Orleans", "3")})
                                  .ToQueryBuilder()
                                  .AndInteger("ID")
                                  .AndString("CD_USUARIO");
        __usuario = __repo.LoadOne(new Usuario(), 
                                    __repo.ExecuteNamedReader("Dal.Repositories.table_name.SelectAll",
                                    builder));
        builder.Clear()
               .UseParam("ID", "6")
               .AndInteger("ID");
        __usuario = __repo.LoadOne(new Usuario(),
                                    __repo.ExecuteNamedReader("Dal.Repositories.table_name.SelectAll", builder));
        // =======================================================================================================
        // Cargar varios elemento reutilizando QueryBuilder
        // =======================================================================================================
        builder.Clear()
               .UseParam("Ids", "6-5")
               .AndListOfIntegers("Ids", "ID");
        var __x1 = __repo.Load( new Usuarios(),
                                __repo.ExecuteNamedReader("Dal.Repositories.table_name.SelectAll",
                                builder));



      }
    }

    private void TestBulkCopy(Dal.Core.DbContext dbContext)
    {
      string[] bulkCopyRows = new string[] { "Rafael Castro;Administrador",
                                             "Rafael Fariñas;Administrador",
                                             "José Martín;Consulta"};
      string mapInfo = "0|#0;1|#1";
      using (IDataReader __dr = new Negocio.Core.Data.CsvDataReader(bulkCopyRows, mapInfo))
      {
        dbContext.BulkCopy("T_NOMBRES",
                           __dr,
                           new string[] { "#0|Nombre",
                                          "#1|Descripcion" });
      }
    }
  }

  public class DataContainer
  {
    public int Id { get; set; } = 0;
    public string Name { get; set; } = "";
    public string Tag = "";
  }
}
