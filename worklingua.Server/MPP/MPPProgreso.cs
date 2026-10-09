using System.Collections;
using System.Collections.Generic;
using System.Data;
using worklingua.Server.BE;
using worklingua.Server.DAL;
using worklingua.Server.Services;

namespace worklingua.Server.MPP
{
    public class MPPProgreso
    {
        Acceso oDatos;
        Hashtable Hdatos;

        public MPPProgreso()
        {
            oDatos = new Acceso();
        }

        public List<BECursoConProgreso> ListarPorUsuario(BEUsuario Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);

            DataTable Dt = oDatos.Leer("sp_Progreso_ListarPorUsuario", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                List<BECursoConProgreso> ListaCursoBE = new List<BECursoConProgreso>();

                foreach (DataRow Item in Dt.Rows)
                {
                    AgregarFila(ListaCursoBE, Item);
                }

                return ListaCursoBE;
            }
            else
            {
                return null;
            }
        }

        public List<BEProgresoUsuario> ListarPorEmpresa(BEEmpresa Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@EmpresaId", Objeto.EmpresaId);

            DataTable Dt = oDatos.Leer("sp_Progreso_ListarPorEmpresa", Hdatos);

            if (Dt.Rows.Count > 0)
            {
                List<BEProgresoUsuario> ListaUsuarioBE = new List<BEProgresoUsuario>();
                BEProgresoUsuario oActual = null;

                foreach (DataRow Item in Dt.Rows)
                {
                    int usuarioId = ServicioLectorFila.LeerEntero(Item, "UsuarioId");

                    if (oActual == null || oActual.UsuarioId != usuarioId)
                    {
                        oActual = new BEProgresoUsuario(
                            usuarioId,
                            ServicioLectorFila.LeerTexto(Item, "Usuario"),
                            ServicioLectorFila.LeerTexto(Item, "Email"),
                            new List<BECursoConProgreso>());

                        ListaUsuarioBE.Add(oActual);
                    }

                    AgregarFila(oActual.Cursos, Item);
                }

                return ListaUsuarioBE;
            }
            else
            {
                return null;
            }
        }

        public int Guardar(BEProgreso Objeto)
        {
            Hdatos = new Hashtable();
            Hdatos.Add("@UsuarioId", Objeto.UsuarioId);
            Hdatos.Add("@ModuloId", Objeto.ModuloId);
            Hdatos.Add("@PorcentajeAvance", Objeto.PorcentajeAvance);
            Hdatos.Add("@Estado", Objeto.Estado);
            Hdatos.Add("@FechaInicio", Objeto.FechaInicio);
            Hdatos.Add("@FechaFinalizacion", Objeto.FechaFinalizacion);

            object identidad = oDatos.LeerEscalar("sp_Progreso_Guardar", Hdatos);

            return Convert.ToInt32(identidad);
        }

        private void AgregarFila(List<BECursoConProgreso> ListaCursoBE, DataRow Item)
        {
            int cursoId = ServicioLectorFila.LeerEntero(Item, "CursoId");
            BECursoConProgreso oCurso = ListaCursoBE.Count > 0 ? ListaCursoBE[ListaCursoBE.Count - 1] : null;

            if (oCurso == null || oCurso.Curso.CursoId != cursoId)
            {
                BECurso oCursoBE = new BECurso();
                oCursoBE.CursoId = cursoId;
                oCursoBE.Nombre = ServicioLectorFila.LeerTexto(Item, "Curso");
                oCursoBE.Idioma = ServicioLectorFila.LeerTexto(Item, "Idioma");
                oCursoBE.Nivel = ServicioLectorFila.LeerTexto(Item, "Nivel");
                oCursoBE.DuracionHoras = ServicioLectorFila.LeerEnteroNulo(Item, "DuracionHoras");
                oCursoBE.Descripcion = ServicioLectorFila.LeerTextoNulo(Item, "Descripcion");
                oCursoBE.Sector = ServicioLectorFila.LeerTextoNulo(Item, "Sector");
                oCursoBE.FechaPublicacion = ServicioLectorFila.LeerFechaNula(Item, "FechaPublicacion");
                oCursoBE.FechaFin = ServicioLectorFila.LeerFechaNula(Item, "FechaFin");
                oCursoBE.Activo = true;

                oCurso = new BECursoConProgreso();
                oCurso.Curso = oCursoBE;
                ListaCursoBE.Add(oCurso);
            }

            if (!ServicioLectorFila.TieneValor(Item, "ModuloId"))
            {
                return;
            }

            BEModulo oModuloBE = new BEModulo(
                ServicioLectorFila.LeerEntero(Item, "ModuloId"),
                cursoId,
                ServicioLectorFila.LeerTexto(Item, "Modulo"),
                ServicioLectorFila.LeerTextoNulo(Item, "ModuloDescripcion"),
                ServicioLectorFila.LeerTextoNulo(Item, "ModuloContenido"),
                ServicioLectorFila.LeerEntero(Item, "OrdenModulo"),
                true);

            BEProgreso oProgresoBE = null;

            if (ServicioLectorFila.TieneValor(Item, "ProgresoId"))
            {
                oProgresoBE = new BEProgreso(
                    ServicioLectorFila.LeerEntero(Item, "ProgresoId"),
                    ServicioLectorFila.LeerEntero(Item, "UsuarioId"),
                    oModuloBE.ModuloId,
                    ServicioLectorFila.TieneValor(Item, "PorcentajeAvance")
                        ? ServicioLectorFila.LeerDecimal(Item, "PorcentajeAvance")
                        : (decimal?)null,
                    ServicioLectorFila.LeerFechaHoraNula(Item, "FechaInicio"),
                    ServicioLectorFila.LeerFechaHoraNula(Item, "FechaFinalizacion"),
                    ServicioLectorFila.LeerTextoNulo(Item, "Estado"));
            }

            oCurso.Modulos.Add(new BEModuloConProgreso(oModuloBE, oProgresoBE));
        }
    }
}
