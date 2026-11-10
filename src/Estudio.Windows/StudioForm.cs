using Estudio.Core;
using System.Text.Json;

namespace Estudio.Windows;

public sealed class StudioForm : Form
{
    readonly TabControl tabs = new() { Dock = DockStyle.Fill, Padding = new Point(14, 8) };
    readonly Label status = new() { Dock = DockStyle.Bottom, Height = 34, Padding = new Padding(18, 8, 0, 0), Text = "Todo se procesa en tu equipo. No hay cambios automáticos en Windows." };
    static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public StudioForm()
    {
        Text = "Estudio Windows · Herramientas C#";
        Size = new Size(1120, 860);
        MinimumSize = new Size(1000, 760);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        BackColor = Color.FromArgb(245, 247, 241);
        var header = new Panel { Dock = DockStyle.Top, Height = 105, BackColor = Color.FromArgb(23, 59, 55) };
        header.Controls.Add(new Label { Text = "ESTUDIO WINDOWS", ForeColor = Color.FromArgb(214, 231, 168), Font = new Font("Segoe UI", 22, FontStyle.Bold), AutoSize = true, Location = new Point(24, 17) });
        header.Controls.Add(new Label { Text = "Ocho herramientas locales · Diseño, archivos y productividad", ForeColor = Color.WhiteSmoke, AutoSize = true, Location = new Point(27, 64) });
        Controls.Add(tabs); Controls.Add(header); Controls.Add(status);
        BuildNotes(); BuildText(); BuildResize(); BuildContactSheet(); BuildContrast(); BuildGradient(); BuildHash(); BuildFiles();
    }

    FlowLayoutPanel Page(string name, string description)
    {
        var page = new TabPage(name) { BackColor = BackColor };
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(20), FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
        flow.Controls.Add(new Label { Text = description, Width = 920, Height = 55, ForeColor = Color.FromArgb(58, 80, 67) });
        page.Controls.Add(flow); tabs.TabPages.Add(page); return flow;
    }
    static TextBox TextArea(FlowLayoutPanel page, bool readOnly = false, int height = 220)
    {
        var box = new TextBox { Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = false, Width = 920, Height = height, ReadOnly = readOnly, Font = new Font("Consolas", 10) };
        page.Controls.Add(box); return box;
    }
    static TextBox Field(FlowLayoutPanel page, string label, string initial = "")
    {
        page.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 12, 3, 3) });
        var box = new TextBox { Width = 920, Text = initial }; page.Controls.Add(box); return box;
    }
    static NumericUpDown Number(FlowLayoutPanel page, string label, int initial, int min, int max)
    {
        page.Controls.Add(new Label { Text = label, AutoSize = true, Margin = new Padding(3, 12, 3, 3) });
        var input = new NumericUpDown { Minimum = min, Maximum = max, Value = initial, Width = 150 };
        page.Controls.Add(input); return input;
    }
    Button Button(FlowLayoutPanel page, string label, Func<Task> action)
    {
        var button = new Button { Text = label, AutoSize = true, Padding = new Padding(12, 5, 12, 5), BackColor = Color.FromArgb(214, 231, 168), FlatStyle = FlatStyle.Flat, Margin = new Padding(3, 12, 3, 7) };
        button.Click += async (_, _) =>
        {
            button.Enabled = false; status.Text = "Procesando…";
            try { await action(); status.Text = "Operación terminada. Los originales se conservan."; }
            catch (Exception error) { status.Text = "No se completó la operación."; MessageBox.Show(this, error.Message, "Revisa la entrada o la ruta", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            finally { if (!button.IsDisposed) button.Enabled = true; }
        };
        page.Controls.Add(button); return button;
    }
    static string? Open(string filter = "Todos los archivos|*.*")
    {
        using var dialog = new OpenFileDialog { Filter = filter, CheckFileExists = true };
        return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
    }
    static string? Save(string name, string filter)
    {
        using var dialog = new SaveFileDialog { FileName = name, Filter = filter, AddExtension = true, OverwritePrompt = true };
        return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
    }
    static string? Folder()
    {
        using var dialog = new FolderBrowserDialog { Description = "Selecciona la carpeta que quieres consultar", UseDescriptionForTitle = true };
        return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
    }
    const string ImageFilter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp;*.gif";
    void ChooseFile(FlowLayoutPanel page, TextBox field, string filter = "Todos los archivos|*.*") =>
        Button(page, "Elegir archivo…", () => { var path = Open(filter); if (path is not null) field.Text = path; return Task.CompletedTask; });
    void ChooseFolder(FlowLayoutPanel page, TextBox field) =>
        Button(page, "Elegir carpeta…", () => { var path = Folder(); if (path is not null) field.Text = path; return Task.CompletedTask; });

    void BuildNotes()
    {
        var page = Page("Notas", "Escribe o abre texto UTF-8 y guárdalo como una nota nueva. No hay autoguardado: guarda antes de cerrar. Nunca se sustituye un archivo existente.");
        var note = TextArea(page, height: 350); note.Text = "# Mi próxima idea\r\n\r\n## Prioridades\r\n- [ ] \r\n\r\n## Notas\r\n";
        Button(page, "Abrir texto UTF-8…", async () => { var path = Open("Texto y Markdown|*.txt;*.md|Todos los archivos|*.*"); if (path is not null) note.Text = await File.ReadAllTextAsync(path); });
        Button(page, "Guardar nota nueva…", async () => { var path = Save($"nota-{DateTime.Now:yyyy-MM-dd}.md", "Markdown|*.md|Texto|*.txt"); if (path is not null) { var text = note.Text; await Task.Run(() => Toolbox.WriteNew(path, text)); } });
    }
    void BuildText()
    {
        var page = Page("Texto", "Estadísticas, líneas únicas y slug ASCII. El resultado se muestra aparte para conservar tu entrada.");
        var input = TextArea(page, height: 160); input.Text = "Diseño de interfaces\r\nDiseño de interfaces\r\nCrear, probar, mejorar.";
        var output = TextArea(page, true, 150);
        Button(page, "Contar líneas, palabras y caracteres", () => { output.Text = JsonSerializer.Serialize(Toolbox.TextStats(input.Text), JsonOptions); return Task.CompletedTask; });
        Button(page, "Quitar líneas repetidas", () => { output.Text = Toolbox.UniqueLines(input.Text); return Task.CompletedTask; });
        Button(page, "Crear slug", () => { output.Text = Toolbox.Slug(input.Text); return Task.CompletedTask; });
    }
    void BuildResize()
    {
        var page = Page("Imagen PNG", "Reduce una imagen manteniendo proporciones y transparencia. No amplía. Máximo 40 megapíxeles; primer fotograma. Corrige orientación EXIF y exporta PNG sin metadatos.");
        var source = Field(page, "Imagen de origen"); ChooseFile(page, source, ImageFilter);
        var width = Number(page, "Ancho máximo (píxeles)", 1200, 16, 4096);
        Button(page, "Exportar PNG nuevo…", async () => { var target = Save("imagen-reducida.png", "PNG|*.png"); if (target is null) return; string path = source.Text; int value = (int)width.Value; await Task.Run(() => Images.Resize(path, target, value)); });
    }
    void BuildContactSheet()
    {
        var page = Page("Hoja de contacto", "Crea una hoja de miniaturas etiquetadas. Primer nivel, hasta 60 imágenes por nombre. Un archivo dañado detiene la exportación. Los originales se conservan.");
        var source = Field(page, "Carpeta de imágenes"); ChooseFolder(page, source);
        var columns = Number(page, "Columnas", 4, 1, 8);
        Button(page, "Exportar hoja PNG…", async () => { var target = Save("hoja-contacto.png", "PNG|*.png"); if (target is null) return; string path = source.Text; int count = (int)columns.Value; await Task.Run(() => Images.ContactSheet(path, target, count)); });
    }
    void BuildContrast()
    {
        var page = Page("Contraste", "Ratio WCAG de colores sRGB opacos. Texto grande: 18 pt normal o 14 pt en negrita. No es una auditoría de accesibilidad de una interfaz completa.");
        var foreground = Field(page, "Color del texto (#RRGGBB)", "#173B37");
        var background = Field(page, "Color del fondo (#RRGGBB)", "#D6E7A8");
        var sample = new Label { Width = 920, Height = 90, TextAlign = ContentAlignment.MiddleCenter, Text = "El diseño empieza por poder leerlo.", Font = new Font("Segoe UI", 18) }; page.Controls.Add(sample);
        var output = TextArea(page, true, 140);
        Button(page, "Calcular contraste", () => { var result = Toolbox.Contrast(foreground.Text, background.Text); sample.ForeColor = ColorTranslator.FromHtml(foreground.Text); sample.BackColor = ColorTranslator.FromHtml(background.Text); output.Text = JsonSerializer.Serialize(result, JsonOptions); return Task.CompletedTask; });
    }
    void BuildGradient()
    {
        var page = Page("Fondo PNG", "Genera un degradado vertical de alta resolución. Elige colores hexadecimales y exporta un archivo nuevo. No aplica el fondo ni cambia preferencias de Windows.");
        var top = Field(page, "Color superior", "#10152E"); var bottom = Field(page, "Color inferior", "#46C9B5");
        var width = Number(page, "Ancho", 1920, 64, 7680); var height = Number(page, "Alto", 1080, 64, 4320);
        Button(page, "Exportar fondo PNG…", async () => { Toolbox.ColorHex(top.Text); Toolbox.ColorHex(bottom.Text); var a = ColorTranslator.FromHtml(top.Text); var b = ColorTranslator.FromHtml(bottom.Text); int w = (int)width.Value, h = (int)height.Value; var target = Save("fondo-degradado.png", "PNG|*.png"); if (target is not null) await Task.Run(() => Images.Gradient(target, a, b, w, h)); });
    }
    void BuildHash()
    {
        var page = Page("SHA-256", "Calcula la huella de un archivo en bloques. Compara con la huella publicada por una fuente fiable; la huella por sí sola no autentica al autor.");
        var source = Field(page, "Archivo"); ChooseFile(page, source);
        var output = TextArea(page, true, 150);
        Button(page, "Calcular SHA-256", async () => { string path = source.Text; output.Text = await Task.Run(() => Toolbox.Hash(path)); });
    }
    void BuildFiles()
    {
        var page = Page("Archivos", "Inventario CSV y duplicados SHA-256. Recorrido recursivo que omite enlaces/junctions. No elimina ni renombra archivos. Las rutas del informe pueden ser privadas.");
        var source = Field(page, "Carpeta"); ChooseFolder(page, source);
        var output = TextArea(page, true, 240);
        FileRow[]? rows = null;
        Button(page, "Crear inventario", async () => { string path = source.Text; rows = await Task.Run(() => Toolbox.Inventory(path)); output.Text = Toolbox.InventoryCsv(rows); });
        Button(page, "Exportar inventario CSV…", async () => { if (rows is null) throw new ArgumentException("Crea primero un inventario."); var csv = Toolbox.InventoryCsv(rows); var target = Save("inventario.csv", "CSV|*.csv"); if (target is not null) await Task.Run(() => Toolbox.WriteNew(target, csv)); });
        Button(page, "Buscar duplicados sin borrar", async () => { string path = source.Text; var groups = await Task.Run(() => Toolbox.Duplicates(path)); output.Text = JsonSerializer.Serialize(groups, JsonOptions); });
    }
}
