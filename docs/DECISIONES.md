# Decisiones — fork de GtkSharp (.NET 10)

Última actualización: 2026-11-22

Registro de las decisiones del fork `Armando1424/GtkSharp` respecto a upstream,
con el motivo y la alternativa descartada.

---

## D1. Migrar a .NET 10 en una rama aparte (`feature/net10`)

**Contexto.** Upstream seguía en .NET 8 y el resto del stack (maui-linux,
CoreLibraries, apps) ya estaba en .NET 10.

**Decisión.** Rama `feature/net10` apilada sobre `feature/linux-arm64`, sin tocar
`develop`/`master` de upstream.

**Consecuencia.** Todo el trabajo vive en el fork; upstream sigue intacto y el PR
#477 (ARM64) continúa pendiente.

---

## D2. `net10.0` + `netstandard2.0` como TFMs

**Contexto.** Hay consumidores en .NET 8 y en .NET 10.

**Decisión.** Las librerías multi-targetean `net10.0` y `netstandard2.0`; el
workload expone solo `net10.0-gtk`.

**Consecuencia.** Un consumidor en .NET 8 sigue funcionando con el ensamblado
`netstandard2.0`, aunque el workload no.

---

## D3. Cake 2.0 → 6.3.0

**Contexto.** Cake 2.0 no instala addins sobre .NET 10 (faltan assemblies
satélite `en-GB`) y fallaba el build.

**Decisión.** Subir a Cake 6.3.0.

**Consecuencia.** El build funciona con un solo SDK; los addins que no se usaban
se eliminaron en el port de GTK 4.

---

## D4. Bandas de workload: 10.0.x + la del SDK instalado

**Contexto.** El CI y las máquinas locales tienen bandas distintas
(10.0.400, 10.0.100…), y el manifiesto solo se empaquetaba para una.

**Decisión.** Empaquetar 10.0.100–10.0.400 y **añadir la banda del SDK actual**
en `Init` si no está.

**Consecuencia.** `--BuildTarget=InstallWorkload` funciona en cualquier SDK 10
sin editar el `build.cake`.

---

## D5. El generador debe pasar `NULL`, no un `GtkTreeIter` en cero

**Contexto.** El generador traducía el parámetro `parent` de
`IterChildren`/`IterNthChild`/`IterNChildren` como un puntero a un iterador
vacío, cuando la API espera `NULL`.

**Decisión.** Añadir un atributo `nullable` en `Gapi.xsd`, manejarlo en
`Parameter.cs` y declarar las reglas en `GtkSharp.metadata`.

**Alternativa descartada.** Parchear a mano el código generado (se perdería en la
siguiente generación).

**Consecuencia.** 7 tests de API nuevos y 191 tests verdes en ARM64.

---

## D6. Cobertura: subir a 30 % antes de firmar la fase 5

**Contexto.** La fase 5 exigía 30 % de cobertura y el código a mano estaba en
21,4 %.

**Decisión.** Añadir 53 tests centrados en lo que no estaba cubierto (`KeyFile`,
`Value`/`Variant`, fechas, `IOChannel`, `Gdk.Rectangle/Pixbuf`, `TreeStore`,
`ListStore`, `PtrArray`, `Builder`), con coverlet excluyendo `**/Generated/**`.

**Consecuencia.** 30,31 % y 244 tests; el número es comparable entre ramas
porque el código generado no cuenta.

---

## D7. Los tests que necesitan display usan `[GtkFact]`

**Contexto.** El CI y la Pi corren sin sesión gráfica.

**Decisión.** Un atributo `[GtkFact]` que se salta el test cuando no hay display,
en vez de fallar.

**Consecuencia.** `dotnet test` pasa en cualquier entorno; en la Pi y en CI se
ejecuta con `xvfb-run` para que corran de verdad.
