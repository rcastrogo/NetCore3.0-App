using Negocio;
using Dal.Core;
using Dal.Core.Loader;
using Dal.Utils;
using Microsoft.VisualBasic.CompilerServices;
using System.Collections.Generic;
using System.Data;

namespace Dal.Repositories
{
    [RepoName("Dal.Repositories.SampleUsuariosRepository")]
    public class SampleUsuariosRepository : RepositoryBase
    {
        public SampleUsuariosRepository() : base(new DbContext())
        {
        }

        public SampleUsuariosRepository(DbContext context) : base(context)
        {
        }

        public IDataReader GetItems(Dictionary<string, string> @params)
        {
            QueryBuilder builder = new QueryBuilder(@params);
            builder.AndStringLike("Nombre");
            return GetItems(builder.ToQueryString());
        }

        public int Insert(string codigo, string descripcion)
        {
            string[] values = new string[] { Helper.ParseString(codigo), 
                                             Helper.ParseString(descripcion) };
            return Insert(values);
        }

        public int Update(int id, string codigo, string descripcion)
        {
            string[] values = new string[] { Conversions.ToString(id), 
                                             Helper.ParseString(codigo),
                                             Helper.ParseString(descripcion),
                                           };
            return Update(values);
        }

    }

}

namespace Negocio.Entities
{
    using Dal.Core;
    using Dal.Repositories;
    using Negocio.Core;
    using System;

    [Serializable]
    public class SampleUsuario : Entity
    {
        
        public SampleUsuario()
        {
            _codigo = "";
            _descripcion = "";
        }

        public SampleUsuario(DbContext context) : base(context)
        {
            _codigo = "";
            _descripcion = "";
        }

        public SampleUsuario Load(int id)
        {           
            SampleUsuariosRepository objA = new SampleUsuariosRepository(DataContext);
            try
            {
                return objA.LoadOne<SampleUsuario>(this, objA.GetItem(id));
            }
            finally
            {
                if (!ReferenceEquals(objA, null))
                {
                    ((IDisposable) objA).Dispose();
                }
            }
        }

        public void Delete()
        {
          using (SampleUsuariosRepository objA = new SampleUsuariosRepository(DataContext))
          {
            objA.Delete(_id);
          }        
        }

        public SampleUsuario Save()
        {
            SampleUsuario animal;
            SampleUsuariosRepository objA = new SampleUsuariosRepository(DataContext);
            try
            {
                if (_id == 0)
                {
                    _id = objA.Insert(Codigo, Descripcion);
                }
                else
                {
                    objA.Update(Id, Codigo, Descripcion);
                }
                animal = this;
            }
            finally
            {
                if (!ReferenceEquals(objA, null))
                {
                    ((IDisposable) objA).Dispose();
                }
            }
            return animal;
        }

        public override string ToString()
        {
            return (Id.ToString() + " " + Descripcion.ToString());
        }
                        
        private int _id;
        public override int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        private string _codigo = "";
        public string Codigo
        {
            get
            {
                return _codigo;
            }
            set
            {
                _codigo = value;
            }
        }

        private string _descripcion = "";
        public string Descripcion
        {
            get { return _descripcion; }
            set { _descripcion = value; }
        }


    }
}
