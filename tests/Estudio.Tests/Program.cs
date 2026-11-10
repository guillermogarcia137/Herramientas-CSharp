using Estudio.Core;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    Console.WriteLine("OK: " + message);
}
var root = Path.Combine(Path.GetTempPath(), "estudio-tests-" + Guid.NewGuid());
Directory.CreateDirectory(root);
try
{
    var a = Path.Combine(root, "a.txt"); var b = Path.Combine(root, "b.txt");
    Toolbox.WriteNew(a, "hello"); Toolbox.WriteNew(b, "hello");
    Check(Toolbox.Hash(a) == "2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824", "SHA-256 de referencia");
    Check(Toolbox.Duplicates(root).Length == 1, "Duplicados sin borrar");
    var inventory = Toolbox.Inventory(root);
    Check(inventory.Length == 2 && inventory.All(row => row.Bytes == 5), "Inventario de tamaños y rutas");
    bool collision = false;
    try { Toolbox.WriteNew(a, "no sobrescribir"); } catch (IOException) { collision = true; }
    Check(collision && File.ReadAllText(a) == "hello", "Preserva archivos existentes");
    Check(Toolbox.Contrast("#000000", "#FFFFFF").Ratio == 21, "Contraste blanco/negro");
    Check(Toolbox.Contrast("#123456", "#123456").Ratio == 1, "Contraste de colores iguales");
    Check(Toolbox.Slug("Diseño de interfaces") == "diseno-de-interfaces", "Slug con acentos");
    Check(Toolbox.UniqueLines("uno\r\nuno\r\ndos") == string.Join(Environment.NewLine, "uno", "dos"), "Líneas únicas con orden estable");
    Check(Toolbox.TextStats("uno dos\n\nfin\n") == new TextReport(3, 3, 13), "Estadísticas de texto");
    var csv = Toolbox.InventoryCsv([new FileRow("una,\"cita\".txt", 12, DateTime.UnixEpoch)]);
    Check(csv.Contains("\"una,\"\"cita\"\".txt\""), "CSV escapa comas y comillas");
}
finally { Directory.Delete(root, true); }
