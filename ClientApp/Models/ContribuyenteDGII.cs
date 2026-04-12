using System.Text.Json.Serialization;

namespace ClientApp.Models
{
    public class ContribuyenteDGII
    {
        public string? RNC { get; set; }
        public string? NombreCompleto { get; set; }
        public string? NombreComercial { get; set; }
        public string? Actividad { get; set; }
        public string? FechaRegistro { get; set; }
        public string? Estado { get; set; }
        public string? Categoria { get; set; }


        // Campos internos para búsqueda (no se retornan al cliente)
        [JsonIgnore] 
        public string NombreCompletoNormalizado { get; set; }
        [JsonIgnore]
        public string NombreComercialNormalizado { get; set; }
    }
}
