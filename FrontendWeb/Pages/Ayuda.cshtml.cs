using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;

namespace FrontendWeb.Pages;

public class AyudaModel : PageModel
{
    private readonly IWebHostEnvironment _environment;

    public AyudaModel(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public IActionResult OnGetDescargar()
    {
        // Construye la ruta relativa y la resuelve a ruta absoluta para diagnóstico en Windows
        string rutaRelativa = Path.Combine(_environment.ContentRootPath, "..", "Documentacion", "Ensayo_Proyecto3_202300583.pdf");
        string rutaAbsoluta = Path.GetFullPath(rutaRelativa);

        if (!System.IO.File.Exists(rutaAbsoluta))
        {
            return Content($"Error: No se encontró el archivo en {rutaAbsoluta}");
        }

        return PhysicalFile(rutaAbsoluta, "application/pdf", "Ensayo_Proyecto3_202300583.pdf");
    }
}