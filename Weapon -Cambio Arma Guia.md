
# Refactorización del sistema de armas

## Resumen

El sistema de armas pasó de estar acoplado al AK-47 a un diseño **data-driven** con ScriptableObjects. Las stats de cada arma viven en assets separados; el jugador usa los mismos componentes para todas.

---

## Archivos nuevos

| Archivo                                    | Descripción                                    |
| ------------------------------------------ | ---------------------------------------------- |
| `Assets/Scripts/Player/WeaponData.cs`      | ScriptableObject con stats de cada arma        |
| `Assets/Scripts/Player/WeaponInventory.cs` | Lista de armas del jugador y cambio opcional   |
| `Assets/Guns/AK-47/AK47_WeaponData.asset`  | Datos del AK-47 (migración del setup anterior) |
| `Assets/Scripts/Vehicles/VehicleData.cs`   | *(Opcional)* Stats de vehículos por asset      |

---

## Archivos modificados

### `WeaponEquipController.cs`

| Antes                                                 | Ahora                                                                                     |
| ----------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| `ak47Model`                                           | `weaponModel`                                                                             |
| Stats hardcodeadas (`magazineSize`, `fireRate`, etc.) | Lee de `WeaponData`; los campos públicos quedan como **fallback** si `weaponData` es null |
| Sin inventario                                        | Referencia a `WeaponInventory` y escucha `OnWeaponChanged`                                |

**Comportamiento sin cambios:**
- **F** — equipar / recoger
- **X** — soltar
- **Click izquierdo** — disparar
- **Click derecho** — apuntar
- Animaciones: `IsAiming`, `IsFiring`, capa `Weapon Layer`

**Nuevo:** soporte para `FireMode.Semi` (un disparo por click) y `FireMode.Auto` (mantener click).

### `TrackVell.cs`

- Campo opcional `vehicleData` (`VehicleData`).
- Si está asignado, copia velocidades y aceleraciones en `Awake()`.
- Si no hay asset, usa los valores del Inspector (comportamiento anterior).

### `Assets/Scenes/ES(ACTU).unity`

- Player actualizado: `weaponModel`, `weaponData`, componente `WeaponInventory`.

---

## Estructura de `WeaponData`

Crear desde: **clic derecho → Create → TTHS → Weapon Data**

| Campo          | Tipo       | Descripción                                    |
| -------------- | ---------- | ---------------------------------------------- |
| `displayName`  | string     | Nombre visible (ej. "AK-47")                   |
| `worldPrefab`  | GameObject | Prefab del modelo en el mundo (suelo, pickups) |
| `magazineSize` | int        | Balas por cargador                             |
| `fireRate`     | float      | Disparos por segundo                           |
| `reloadTime`   | float      | Segundos de recarga                            |
| `shootRange`   | float      | Alcance del raycast                            |
| `fireMode`     | enum       | `Semi` o `Auto`                                |

---

## Cómo agregar una nueva arma

### Paso 1: Crear el asset de datos

1. Clic derecho en `Assets/Guns/` (o carpeta de la arma).
2. **Create → TTHS → Weapon Data**.
3. Nómbralo, por ejemplo `Pistol_WeaponData`.
4. Configura stats en el Inspector.

### Paso 2: Modelo 3D

- Importa el modelo en la escena o como prefab.
- Asigna el prefab a `worldPrefab` en el `WeaponData`.
- Si el arma va en la mano del jugador, arrastra el modelo a **Weapon Model** en `WeaponEquipController`.

### Paso 3: Registrar en el inventario

1. Selecciona el **Player** en la escena.
2. En `WeaponInventory`, añade el nuevo `WeaponData` a la lista **Weapons**.
3. Ajusta **Current Weapon Index** si quieres que empiece con esa arma.

### Paso 4: Cambio de arma (opcional)

Con **una sola arma:** deja `Enable Weapon Switching` en **false**.

Con **varias armas:**
1. Activa **Enable Weapon Switching**.
2. Controles:
   - **Scroll** del mouse
   - **E** — siguiente
   - **Q** — anterior
   - **1–9** — arma directa

> **Nota:** **E** también entra/sale del carro (`VehicleEntry`). Si usas cambio de arma con E, considera cambiar `nextWeaponKey` en `WeaponInventory` o `interactKey` en `VehicleEntry`.

### Paso 5: Animator 

- Deje las mismas animaciones: `IsAiming`, `IsFiring`, capa `Weapon Layer`.
- Armas con animaciones distintas: otro `RuntimeAnimatorController` por arma (futuro; hoy comparten el mismo Animator).

---

## Checklist rápido

- [x] Asset `WeaponData` creado con stats correctas
- [x] `worldPrefab` asignado
- [x] Arma en la lista de `WeaponInventory`
- [x] `WeaponEquipController.weaponData` apunta al asset (o se toma del inventario)
- [x] `weaponModel` apunta al modelo en la mano
- [x] Probar: equipar, disparar, recargar, soltar, recoger

---

## Migración desde el setup anterior

Si hay otra escena o prefab con el setup viejo:

1. Renombrar `ak47Model` → **Weapon Model** (Unity puede mostrar campo vacío).
2. Crea o asigna un asset `WeaponData`.
3. Añadir `WeaponInventory` al Player y conecta la referencia en `WeaponEquipController`.

---

## Qué no hice cambiós(Posiblemente tenga que actualizarlo)

- Lógica de movimiento del jugador (`PlayerController`)
- Entrar/salir del carro (`VehicleEntry`)
- Física y marchas del auto (`CarController`, `TrackVell`)
- Ragdoll al salir del carro en movimiento