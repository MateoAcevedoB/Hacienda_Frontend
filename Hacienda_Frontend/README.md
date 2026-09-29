# Hacienda TOBE — Frontend Angular

SPA en Angular 21 (standalone components, signals, formularios reactivos) que consume `Api_Hacienda`.

## Requisitos
- Node 20+ (probado con Node 22 y npm 10)
- Backend `Api_Hacienda` en `http://localhost:5175` (ver `src/environments/environment*.ts`), con PostgreSQL y en modo Development. CORS ya permite `http://localhost:4200`.

## Comandos
```bash
npm ci            # instala (el .npmrc activa legacy-peer-deps, necesario para vitest/jsdom)
npm start         # ng serve → http://localhost:4200
npm run build     # build de producción
npm test          # ng test con Vitest (jsdom), una sola ejecución: npx ng test --no-watch
```

## Módulos y endpoints reales usados
| Pantalla | Ruta | Endpoints |
|---|---|---|
| Login | `/login` | `POST /api/auth/login` |
| Dashboard | `/dashboard` | `GET /api/potreros`, `/api/ventas`, `/api/vacunas` (un fallo parcial no rompe el panel) |
| Potreros | `/potreros` | `GET/POST /api/potreros` |
| Detalle de potrero | `/potreros/:id` | `GET /api/potreros/{id}`, `GET/POST /api/potreros/{id}/reses`, `POST .../reses/{nombre}/vender` |
| Ventas | `/ventas` | `GET /api/ventas` (filtro `potreroId` **o** `desde`+`hasta`, nunca combinados) |
| Vacunas | `/vacunas` | `GET/POST /api/vacunas`, `POST /api/vacunacion/aplicar` |
| Usuarios (solo admin) | `/usuarios` | `GET/POST /api/usuarios` |

`GET /api/potreros/{id}/reses/{nombre}` está disponible en `ResService.obtenerRes` pero ninguna pantalla lo necesita todavía.

## Autenticación y roles
- El JWT y su `expiresAt` se guardan en `localStorage`; `authInterceptor` agrega el Bearer y cierra sesión ante un 401; `errorInterceptor` traduce los errores del backend a mensajes.
- En el backend el **rol es el nombre de usuario** (`admin`, `empleado`, `visitante`). `PermisosService` decide qué se muestra: admin y empleado pueden escribir; visitante (y cualquier otro rol) solo consulta; `/usuarios` solo admin (`adminGuard`).
- Ocultar botones es solo cosmético: la autorización real la hace la API. Según el código revisado, los controladores solo exigen `[Authorize]` (y `Roles="admin"` en usuarios), así que un visitante podría escribir llamando a la API directamente.

## Reglas que respeta la UI (vienen del backend)
- Vacuna: exactamente uno de `periodoAplicacion` (bacteriana, 2–4 semanas) o `atenuacionGrado` (viva: 10, 20 o 30); vencimiento posterior a la aplicación.
- Ventas: `hasta` se envía como fin de día (`T23:59:59`) para incluir el día completo.
- Reses: los mínimos de peso/edad por tipo se muestran como ayuda; la validación real es del servidor.

## Fuera de alcance
Alimentación e historia clínica existen en el MVC (`p_mvcHacienda`) pero **la API no expone endpoints** para ellas, por lo que no se implementaron.

## Pruebas
43 pruebas automatizadas (servicios, interceptores, guards, permisos y componentes) con **HTTP simulado** (`HttpTestingController`). Verifican los contratos según los DTO del backend, pero **no son pruebas de integración**: no se ejecutaron contra la API real.

Checklist manual de integración (con API y PostgreSQL levantados):
1. Login con `admin`, `empleado` y `visitante`; comprobar el menú y los botones por rol.
2. Crear potrero → agregar res → vender res → ver la venta en `/ventas` (con filtro por potrero y por fechas).
3. Crear vacuna bacteriana y viva → aplicarla a una res.
4. Crear usuario como admin; comprobar que un no-admin no ve `/usuarios`.
5. Cerrar el backend y confirmar el mensaje de "sin conexión".

## Estructura
```
src/app/
  core/{models,services,guards,interceptors,errors}
  layout/shell.*        # barra de navegación por rol
  pages/{login,dashboard,potreros,potrero-detalle,ventas,vacunas,usuarios}
  testing/helpers.ts    # utilidades de pruebas
src/styles.css          # tema (paleta verde de site.css del MVC) y utilidades .aw-*
```
