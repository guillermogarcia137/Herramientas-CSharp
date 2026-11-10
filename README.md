# Herramientas-CSharp · Estudio Windows

Aplicación de escritorio para Windows 10/11, escrita en C# y Windows Forms.
Ocho pestañas reúnen diseño, imágenes, texto y análisis de archivos. No necesita
cuentas ni conexión durante su uso. No modifica la configuración de Windows.

| Pestaña | Funciones |
| --- | --- |
| Notas | Abrir UTF-8, escribir Markdown y guardar un archivo nuevo |
| Texto | Estadísticas, líneas únicas y slug ASCII |
| Imagen PNG | Reducir sin ampliar, conservar proporciones y transparencia |
| Hoja de contacto | Miniaturas etiquetadas de hasta 60 imágenes |
| Contraste | Ratio WCAG para colores opacos y vista previa |
| Fondo PNG | Degradados verticales de hasta 7680 × 4320 |
| SHA-256 | Huellas de archivos en bloques |
| Archivos | Inventario CSV y búsqueda de duplicados sin borrado |

## Compilar y ejecutar

Instala el SDK .NET 10 oficial en Windows y ejecuta desde la raíz:

```console
dotnet run --project tests/Estudio.Tests -c Release
dotnet run --project src/Estudio.Windows -c Release
```

Para generar un ejecutable que incluya el runtime:

```console
dotnet publish src/Estudio.Windows -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist
```

Abre `dist/EstudioWindows.exe`. La primera apertura de un ejecutable autocontenido
puede tardar al extraer componentes. No se firma el ejecutable; Windows puede
mostrar advertencias de reputación. El workflow de GitHub Actions publica los
archivos compilados como artefacto de la ejecución, no como una Release.

## Comportamiento

Los destinos deben ser nuevos: una ruta existente se rechaza aunque el diálogo
permita seleccionarla. Si una escritura falla puede quedar una salida parcial;
revísala y usa otro nombre. No hay autoguardado: guarda tus notas antes de cerrar.
Los originales se conservan. Las tareas pesadas se ejecutan fuera del hilo de la
interfaz, sin cancelación intermedia; no cierres la aplicación durante una exportación.

Las imágenes se limitan a 40 megapíxeles de entrada; se aplica orientación EXIF
y se exportan píxeles sin metadatos ni animación. No admite SVG ni formatos no
soportados por System.Drawing. Los recorridos omiten enlaces/junctions y fallan
si hay archivos inaccesibles; no garantizan consistencia ante cambios concurrentes.
Los CSV conservan datos literales: al abrir archivos no confiables en Excel,
impórtalos como texto para evitar interpretación de fórmulas.

## Validación

Las pruebas del núcleo se ejecutan con archivos temporales sin librerías de pruebas
externas y funcionan también en Linux. Se ha compilado la aplicación para Windows
desde Linux; la interfaz gráfica y las operaciones System.Drawing requieren una
comprobación manual en Windows. El workflow repite pruebas y publicación en Windows.
