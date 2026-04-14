# GUÍA DE IMPLEMENTACIÓN - Migración de Datos GoodHard

## Descripción del Proyecto
Esta aplicación realiza la migración de datos desde archivos de texto a una base de datos Access, cumpliendo con los requisitos solicitados por GoodHard, distribuidor mayorista de partes de computadoras.

## Estructura Implementada

### 1. Base de Datos (Distribuidora.mdb)
Se crea automáticamente con las siguientes tablas:

#### Tabla: Categorias
- **IdCategoria** (INT, PRIMARY KEY): Identificador único de categoría
- **Nombre** (TEXT): Nombre de la categoría

#### Tabla: Articulos
- **IdArticulo** (INT, PRIMARY KEY): Identificador único de artículo
- **Nombre** (TEXT): Nombre del artículo
- **IdCategoria** (INT, FOREIGN KEY): Referencia a Categorias
- **Precio** (CURRENCY): Precio del artículo

### 2. Archivos de Datos de Entrada

#### Categorias.txt
Formato: IdCategoria TAB Nombre
```
1	Procesadores
2	Disco Rígido
3	Gabinete PC
4	Fuentes
5	DVD
6	Teclados
```

#### Articulos.txt
Formato: IdArticulo TAB Nombre TAB IdCategoria TAB Precio
```
100	HD 80 GB	2	190
101	Teclado 102	2	25.75
103	HD 120 GB SATA	2	105
... (más artículos)
```

### 3. Clases Implementadas

#### clsConexionDB
- Gestiona la conexión a la base de datos Access
- Soporta archivos .accdb y .mdb
- Implementa IDisposable para liberar recursos
- Método público `GetConnectionString()` para obtener la cadena de conexión

#### clsMigracion
- Crea la estructura de tablas en la base de datos
- Migra datos desde archivos de texto a tablas
- Valida datos durante la inserción
- Mantiene un log detallado de todas las operaciones
- Maneja errores individualmente para cada registro

#### frmPrincipal
- Formulario principal con interfaz gráfica
- Botón "Iniciar Migración" para comenzar el proceso
- TextBox de solo lectura que muestra el log detallado
- Muestra errores y advertencias en tiempo real

## Flujo de Ejecución

1. **Verificación de Base de Datos**: Si no existe Distribuidora.mdb, se crea automáticamente
2. **Creación de Estructura**: Se crean las tablas Categorias y Articulos
3. **Migración de Categorías**: Se leen datos de Categorias.txt y se insertan en la tabla
4. **Migración de Artículos**: Se leen datos de Articulos.txt y se insertan en la tabla
5. **Reporte**: Se muestra un log completo con la cantidad de registros insertados

## Manejo de Errores

- Validación de archivos (existencia y formato)
- Validación de tipos de datos durante la inserción
- Logs detallados de errores individuales
- Los errores en un registro no detienen la migración
- Manejo de excepciones con mensajes descriptivos

## Características de la Interfaz

- **TextBox de Log**: Muestra todas las operaciones en tiempo real
- **Scroll automático**: Se desplaza automáticamente al final del log
- **Botón deshabilitado durante migración**: Evita clics múltiples
- **Mensajes visuales**: Incluye símbolos de éxito (?) y error (?)
- **Información detallada**: Rutas, cantidad de registros, etc.

## Ubicación de Archivos

Al compilar y ejecutar:
- Distribuidora.mdb se crea en: bin/Debug/
- Categorias.txt y Articulos.txt deben estar en: bin/Debug/

## Requisitos Cumplidos

? Crear archivos ARTÍCULOS Y CATEGORÍAS (Articulos.txt, Categorias.txt)
? Crear Base de Datos con tablas y campos necesarios (Distribuidora.mdb)
? Confeccionar código para migrar datos de archivos a base de datos
? Interfaz con caja de texto mostrando detalle de operaciones
? Información de errores que puedan ocurrir
? Sistema robusto de validación y manejo de excepciones
