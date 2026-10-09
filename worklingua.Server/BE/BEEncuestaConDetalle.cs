using System.Collections.Generic;

namespace worklingua.Server.BE
{
    /// <summary>
    /// Encuesta con todo lo que necesita la pantalla: sus opciones con los votos,
    /// el total de respuestas, si sigue vigente y qué votó el usuario de la sesión
    /// (<see cref="OpcionElegidaId"/> en null cuando todavía no respondió).
    /// </summary>
    public class BEEncuestaConDetalle
    {
        #region Propiedades
        public BEEncuesta Encuesta { get; set; }
        public string Idioma { get; set; }
        public string CodigoISO { get; set; }
        public int TotalRespuestas { get; set; }
        public bool Vigente { get; set; }
        public int? OpcionElegidaId { get; set; }
        public List<BEOpcionEncuesta> Opciones { get; set; }
        #endregion

        public BEEncuestaConDetalle()
        {
            this.Opciones = new List<BEOpcionEncuesta>();
        }

        public BEEncuestaConDetalle(
            BEEncuesta encuesta,
            string idioma,
            string codigoISO,
            int totalRespuestas,
            bool vigente,
            int? opcionElegidaId,
            List<BEOpcionEncuesta> opciones)
        {
            this.Encuesta = encuesta;
            this.Idioma = idioma;
            this.CodigoISO = codigoISO;
            this.TotalRespuestas = totalRespuestas;
            this.Vigente = vigente;
            this.OpcionElegidaId = opcionElegidaId;
            this.Opciones = opciones;
        }
    }
}
