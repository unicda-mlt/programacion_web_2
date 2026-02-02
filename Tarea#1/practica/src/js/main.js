window.addEventListener('DOMContentLoaded', async function () {
  await cargarProductos()
  implementarContadorInteractivo()
  implementarBuscadorDePalabras()
})

//#region Lista de Productos desde XML
async function cargarProductos() {
  const $tableBody = document.getElementById('products-table-body')

  const res = await fetch('/src/data/productos.xml')
  const productosXML = await res.text()
  const parser = new DOMParser()
  const xmlDoc = parser.parseFromString(productosXML, 'application/xml')
  const productos = xmlDoc.getElementsByTagName('producto')

  const arrLength = productos.length

  for (let i = 0; i < arrLength; i++) {
    const producto = productos[i]
    const nombre = obtenerTextoEtiqueta(producto, 'nombre')
    const descripcion = obtenerTextoEtiqueta(producto, 'descripcion')
    const precio = obtenerTextoEtiqueta(producto, 'precio')
    const categoria = obtenerTextoEtiqueta(producto, 'categoria')
    const stock = obtenerTextoEtiqueta(producto, 'stock')

    const row = document.createElement('tr')

    row.innerHTML = `
      <td>${nombre}</td>
      <td>${descripcion}</td>
      <td>${precio}</td>
      <td>${categoria}</td>
      <td>${stock}</td>
    `

    $tableBody.appendChild(row)
  }
}

function obtenerTextoEtiqueta(itemXML, etiqueta) {
  return itemXML.getElementsByTagName(etiqueta)[0].textContent
}
//#endregion

//#region Contador Interactivo
function implementarContadorInteractivo() {
  const $btnIncrementar = document.getElementById('btnIncrement')
  const $btnDecrementar = document.getElementById('btnDecrement')
  const $contador = document.getElementById('counterValue')

  let valorContador = 0
  $contador.textContent = valorContador

  $btnIncrementar.addEventListener('click', function () {
    valorContador++
    $contador.textContent = valorContador
  })

  $btnDecrementar.addEventListener('click', function () {
    valorContador--
    $contador.textContent = valorContador
  })
}
//#region

//#region Buscador de Palabras
function implementarBuscadorDePalabras() {
  const $inputBusqueda = document.getElementById('searchInput')
  const $listaPalabras = document.getElementById('wordList')

  const palabras = Array.from($listaPalabras.getElementsByTagName('li'))

  $inputBusqueda.addEventListener('input', function () {
    const terminoBusqueda = $inputBusqueda.value.toLowerCase()

    for (const palabra of palabras) {
      const textoPalabra = palabra.textContent.toLowerCase()

      if (textoPalabra.includes(terminoBusqueda)) {
        palabra.style.display = ''
        continue
      }

      palabra.style.display = 'none'
    }
  })
}
//#endregion
