# Proyecto Web - Demostración de Tecnologías Web Fundamentales

## 📋 Descripción General

Este proyecto educativo (Tarea #1) está diseñado para demostrar las diferencias y aplicaciones prácticas de tres tecnologías fundamentales del desarrollo web: **HTML**, **XML**, **CSS** y **JavaScript**. El proyecto se divide en tres ejercicios principales, cada uno enfocado en mostrar conceptos específicos del desarrollo web moderno.

**Objetivos del Proyecto:**
- Comprender las diferencias entre HTML y XML
- Demostrar el uso de XML para almacenamiento de datos estructurados
- Implementar diseño moderno con CSS3 (Flexbox, variables CSS)
- Crear interactividad dinámica con JavaScript vanilla (ES6+)
- Validar datos XML mediante DTD (Document Type Definition)

---

## 📁 Estructura del Proyecto

```
project/
│
├── README.md                 # Documentación del proyecto
│
└── src/                      # Directorio principal de código fuente
    ├── index.html            # Página principal del proyecto
    │
    ├── assets/               # Recursos multimedia (imágenes, iconos)
    │
    ├── css/                  # Hojas de estilo
    │   └── main.css          # Estilos principales del proyecto
    │
    ├── data/                 # Datos estructurados
    │   ├── productos.xml     # Catálogo de productos en formato XML
    │   └── productos.dtd     # Definición de tipo de documento para validación
    │
    └── js/                   # Scripts JavaScript
        └── main.js           # Lógica principal de la aplicación
```

### Contenido de Cada Carpeta

#### 📄 `src/`
Directorio raíz que contiene todos los archivos del proyecto web.

- **[index.html](src/index.html)**: Archivo HTML principal que estructura las tres secciones del proyecto:
  - Ejercicio 1: Diferencias entre XML y HTML
  - Ejercicio 2: Diseño moderno con CSS
  - Ejercicio 3: Interactividad con JavaScript

#### 🎨 `src/css/`
Contiene las hojas de estilo del proyecto.

- **[main.css](src/css/main.css)**: Archivo CSS principal que incluye:
  - Variables CSS personalizadas para temas consistentes
  - Diseño Flexbox para layouts responsivos
  - Media queries para adaptación móvil (breakpoint: 768px)
  - Efectos hover y transiciones suaves
  - Estilos para tablas, tarjetas de perfil y elementos interactivos

#### 💻 `src/js/`
Contiene los scripts JavaScript del proyecto.

- **[main.js](src/js/main.js)**: Lógica principal de la aplicación que implementa:
  - Carga asíncrona de datos XML mediante Fetch API
  - Parseo de XML usando DOMParser
  - Renderizado dinámico de tabla de productos
  - Contador interactivo (incrementar/decrementar)
  - Filtro de búsqueda en tiempo real
  - Código modular organizado con regiones

#### 📊 `src/data/`
Contiene los archivos de datos estructurados.

- **[productos.xml](src/data/productos.xml)**: Catálogo de 6 productos con estructura:
  - Laptops, periféricos, monitores, webcams y auriculares
  - Cada producto incluye: ID, nombre, descripción, precio, categoría y stock
  - Validado contra el esquema DTD

- **[productos.dtd](src/data/productos.dtd)**: Definición de tipo de documento que:
  - Define la estructura válida del XML
  - Especifica elementos obligatorios (nombre, descripcion, precio, categoria, stock)
  - Establece el atributo `id` como requerido (tipo NMTOKEN)

#### 🖼️ `src/assets/`
Directorio para recursos multimedia como imágenes, iconos y otros archivos estáticos.

---

## ⚙️ Comportamiento del Proyecto

El proyecto está dividido en **tres ejercicios independientes** que demuestran diferentes aspectos del desarrollo web:

### 🔷 Ejercicio 1: XML vs HTML

**Propósito:** Demostrar las diferencias entre XML (almacenamiento de datos) y HTML (presentación).

**Comportamiento:**
1. Al cargar la página, JavaScript realiza una petición asíncrona para obtener [productos.xml](src/data/productos.xml)
2. El archivo XML es parseado usando `DOMParser`
3. Los datos extraídos se transforman dinámicamente en filas de una tabla HTML
4. Se muestra un catálogo de 6 productos con: Nombre, Descripción, Precio, Categoría y Stock

**Productos Incluidos:**
- Laptop HP Pavilion ($699.99)
- Mouse Logitech MX Master ($89.99)
- Teclado Mecánico Corsair ($129.99)
- Monitor Samsung 27" ($249.99)
- Webcam Logitech C920 ($79.99)
- Auriculares Sony WH-1000XM4 ($349.99)

### 🔷 Ejercicio 2: Diseño Moderno con CSS

**Propósito:** Demostrar el uso de CSS3 moderno sin diseños basados en tablas.

**Comportamiento:**
- Tarjeta de perfil para "Juan Pérez" (Desarrollador Web)
- Layout Flexbox con dirección de columna
- Imagen circular de perfil con borde y sombra
- Efecto hover que cambia el color de fondo al color primario
- Diseño completamente responsivo
- Uso de variables CSS para temas consistentes

### 🔷 Ejercicio 3: Interactividad con JavaScript

**Propósito:** Demostrar manipulación del DOM con JavaScript vanilla.

**Funcionalidades:**

1. **Contador Interactivo:**
   - Valor inicial: 0
   - Botón "+" incrementa el contador
   - Botón "-" decrementa el contador
   - Actualización en tiempo real del valor mostrado

2. **Filtro de Palabras:**
   - Lista de 7 palabras tecnológicas: JavaScript, HTML, CSS, XML, JSON, React, Node.js
   - Campo de búsqueda que filtra la lista en tiempo real
   - Búsqueda insensible a mayúsculas/minúsculas
   - Oculta elementos que no coinciden con el término de búsqueda

---

## 🚀 Cómo Ejecutar el Proyecto

### ⚠️ Requisitos Previos

- **Navegador web moderno** (Chrome, Firefox, Edge, Safari)
- **Servidor web local** (obligatorio debido a restricciones CORS)

> **Nota Importante:** Este proyecto **NO puede ejecutarse** directamente abriendo [index.html](src/index.html) con el protocolo `file://` debido a que la Fetch API requiere el protocolo HTTP para cargar archivos XML. Los navegadores modernos bloquean peticiones CORS desde archivos locales por razones de seguridad.

### 📌 Métodos de Ejecución Recomendados

#### **Opción 1: Python HTTP Server** (Recomendado si tienes Python instalado)

```bash
# Navega al directorio del proyecto
cd f:\Desktop\Programacion_Web_2\ProyectoFinal\Tarea#1\project

# Python 3.x
python -m http.server 8000

# Python 2.x (si aplica)
python -m SimpleHTTPServer 8000
```

Luego abre en tu navegador: `http://localhost:8000/src/index.html`

#### **Opción 2: Node.js HTTP Server**

```bash
# Instalar http-server globalmente (solo la primera vez)
npm install -g http-server

# Navega al directorio del proyecto
cd f:\Desktop\Programacion_Web_2\ProyectoFinal\Tarea#1\project

# Iniciar servidor
http-server

# O con puerto específico
http-server -p 8000
```

Luego abre en tu navegador: `http://localhost:8080/src/index.html` (o el puerto indicado)

#### **Opción 3: VS Code Live Server Extension** (Recomendado para desarrollo)

1. Instala la extensión "Live Server" en VS Code
2. Abre el proyecto en VS Code
3. Click derecho en [index.html](src/index.html)
4. Selecciona "Open with Live Server"
5. Se abrirá automáticamente en tu navegador predeterminado

#### **Opción 4: Otros Servidores Web**

- **XAMPP/WAMP:** Coloca el proyecto en la carpeta `htdocs` o `www`
- **PHP:** `php -S localhost:8000` (desde el directorio del proyecto)
- **npx:** `npx serve` (no requiere instalación global)

---

## 🛠️ Tecnologías Utilizadas

- **HTML5**: Estructura semántica del documento
- **CSS3**: Diseño moderno con Flexbox y variables CSS
- **JavaScript (ES6+)**: Lógica interactiva con características modernas
  - Async/await
  - Arrow functions
  - Template literals
  - Destructuring
- **XML**: Almacenamiento de datos estructurados
- **DTD**: Validación de esquema XML
- **Fetch API**: Peticiones HTTP asíncronas
- **DOMParser API**: Parseo de documentos XML

---
