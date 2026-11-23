# Arquitectura de GtkSharp (fork)

Última actualización: 2026-11-22

Alcance: el fork `Armando1424/GtkSharp`, que es una copia de
`GtkSharp/GtkSharp` con la migración a .NET 10. El proyecto original genera los
bindings de C# para GTK 3; aquí se documenta **cómo funciona esa maquinaria**,
que es la misma que después se usa para GTK 4.

## Idea en una línea

GtkSharp no escribe los bindings a mano: **genera** clases de C# a partir de una
descripción XML de la API de GTK (`api.xml`), y esa descripción sale de los
archivos `.gir` de GObject-Introspection. El flujo es:

```text
.gir (GObject Introspection)
   ↓  GapiCodegen + GtkSharp.metadata
api.xml  (descripción de tipos, señales, propiedades)
   ↓  GapiCodegen
Generated/*.cs  (clases C# con P/Invoke a libgtk)
   ↓  dotnet build
GtkSharp.dll + GdkSharp.dll + … (ensamblados por librería)
```

## Estructura

| Ruta | Qué hay |
|---|---|
| `Source/Libs/<Lib>` | Un proyecto por librería enlazada: `GLibSharp`, `GioSharp`, `PangoSharp`, `CairoSharp`, `GdkSharp`, `AtkSharp`, `GtkSharp`, `GtkSourceSharp`, `WebkitGtkSharp` |
| `Source/Libs/<Lib>/<Lib>-api.xml` | Descripción de la API de esa librería (versionada) |
| `Source/Libs/<Lib>/<Lib>.metadata` | Reglas que corrigen el generador: renombrar, ocultar, cambiar tipos |
| `Source/Libs/<Lib>/Generated/` | Código generado (no se edita a mano) |
| `Source/Libs/Shared/` | Código común a todas las librerías (`GLibrary`, `FuncLoader`, marshalling) |
| `Source/Tools/GapiCodegen` | Generador: `api.xml` → C# |
| `Source/Tools/GapiFixup` | Post-proceso del código generado |
| `Source/Workload` | Manifiesto, packs (`GtkSharp.Sdk/Ref/Runtime`), plantillas y TFM `net10.0-gtk` |
| `Source/Tests` | xUnit: comportamiento, API pública y rendimiento |
| `Source/Samples` | App GTK de ejemplo que ejercita los widgets |

## El build (Cake)

`build.cake` orquesta todo. Los objetivos relevantes:

| Objetivo | Qué hace |
|---|---|
| `Prepare` | Compila las tools y **genera** el código desde `api.xml` con los `.metadata` |
| `Build` | Compila la solución de librerías |
| `Test` | xUnit (los tests GTK necesitan display o `xvfb-run`) |
| `TestPerformance` | Benchmarks y tests de memoria |
| `PackageNuGet` | Paquetes de las librerías |
| `PackageWorkload` / `PackageTemplates` / `InstallWorkload` | Packs del workload y su instalación |

El build es **hermético**: no necesita GTK instalado para generar y compilar,
porque `api.xml` está versionado. Solo los tests y los samples tocan GTK nativo.

## Cómo se corrige el generador

El generador no adivina: cuando la API de C no se traduce bien a C#, se corrige
en dos sitios:

1. **`GtkSharp.metadata`** — reglas declarativas: quitar un método, cambiar un
   parámetro a nullable, renombrar, mapear un tipo.
2. **Código escrito a mano** — `Container.cs`, `Widget.cs`, etc. dentro de
   `Source/Libs/GtkSharp`, para lo que ningún metadata puede expresar.

Ejemplo real de este fork: el generador pasaba un puntero a un `GtkTreeIter`
en cero en vez de `NULL` para el padre de `IterChildren`; se corrigió con un
atributo `nullable` en `Gapi.xsd`, manejo en `Parameter.cs` y reglas en el
`.metadata`.

## El workload

El TFM `net10.0-gtk` no lo define .NET: lo registra el manifiesto de GtkSharp
(`GtkSharp.NET.Sdk.Gtk.Manifest-10.0.400`) como un `KnownFrameworkReference`.
Por eso:

- compilar `net10.0-gtk` exige el workload instalado;
- el workload trae `GtkSharp.Ref` (referencias) y `GtkSharp.Runtime` (nativas);
- instalar el workload = manifiesto + packs + registro en
  `metadata/workloads/<banda>/InstalledWorkloads/gtk`.

El fork añade la banda del SDK actual automáticamente para que
`InstallWorkload` funcione con cualquier SDK 10.

## Pruebas

| Tipo | Dónde | Nota |
|---|---|---|
| Comportamiento | `Source/Tests/GtkSharp.Tests` | 244 tests; los que necesitan display usan `[GtkFact]` y se saltan sin X |
| API pública | `PublicAPI/*.txt` | Detecta cambios de superficie accidentalmente |
| Rendimiento/memoria | `Category=Performance` + `GtkSharp.Benchmarks` | Fuera del `Test` normal |

## Relación con el port GTK 4

El port a GTK 4 vive en otra rama y otro repositorio
([`../../GtkSharp-Gtk4/docs/ARQUITECTURA.md`](../../GtkSharp-Gtk4/docs/ARQUITECTURA.md)).
Comparte esta maquinaria: cambia el `.gir` de entrada (Gtk-4.0), añade
`GrapheneSharp`/`GskSharp`/`AdwaitaSharp` y una capa de compatibilidad para
conservar la API de GTK 3.

## Referencias

- [`releasing.md`](releasing.md) — cómo se publica una versión.
- [`testing.md`](testing.md) — cómo se ejecutan las pruebas.
- `../CONTEXT.local.md` — estado local del fork (no versionado).
