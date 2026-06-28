(() => {
  const modules = {
    Alquilers: { plural: "Alquileres", singular: "alquiler" },
    CategoriaHerramienta: { plural: "Categorias", singular: "categoria" },
    Clientes: { plural: "Clientes", singular: "cliente" },
    Devolucion: { plural: "Devoluciones", singular: "devolucion" },
    Herramientas: { plural: "Herramientas", singular: "herramienta" },
    Mora: { plural: "Moras", singular: "mora" },
    Proveedor: { plural: "Proveedores", singular: "proveedor" },
    Reserva: { plural: "Reservas", singular: "reserva" },
    Rol: { plural: "Roles", singular: "rol" },
    Usuario: { plural: "Usuarios", singular: "usuario" }
  };

  const titleByAction = {
    Index: (mod) => mod.plural,
    Create: (mod) => `Nuevo ${mod.singular}`,
    Edit: (mod) => `Editar ${mod.singular}`,
    Details: (mod) => `Detalle de ${mod.singular}`,
    Delete: (mod) => `Eliminar ${mod.singular}`
  };

  const textMap = new Map([
    ["Create New", "Nuevo registro"],
    ["Back to List", "Volver al listado"],
    ["Edit", "Editar"],
    ["Details", "Ver"],
    ["Delete", "Eliminar"],
    ["Save", "Guardar"],
    ["Create", "Guardar"]
  ]);

  function getRouteInfo() {
    const parts = window.location.pathname.split("/").filter(Boolean);
    const controller = parts[0] || "Home";
    const action = parts[1] || "Index";
    return { controller, action };
  }

  function replaceExactText(element) {
    const current = element.textContent.trim();
    if (textMap.has(current)) {
      element.textContent = textMap.get(current);
    }
  }

  function polishTables() {
    document.querySelectorAll(".app-content table.table").forEach((table) => {
      if (!table.parentElement.classList.contains("table-responsive")) {
        const wrapper = document.createElement("div");
        wrapper.className = "table-responsive";
        table.parentNode.insertBefore(wrapper, table);
        wrapper.appendChild(table);
      }
    });
  }

  function polishScaffoldText() {
    const { controller, action } = getRouteInfo();
    const mod = modules[controller];

    if (mod) {
      const h1 = document.querySelector(".app-content > h1:first-child");
      if (h1 && titleByAction[action]) {
        h1.textContent = titleByAction[action](mod);
      }

      const h4 = document.querySelector(".app-content > h4");
      if (h4) {
        h4.textContent = mod.plural;
      }
    }

    document.querySelectorAll(".app-content a").forEach(replaceExactText);
    document.querySelectorAll(".app-content input[type='submit']").forEach((input) => {
      if (textMap.has(input.value)) {
        input.value = textMap.get(input.value);
      }
    });

    document.querySelectorAll(".app-content h3").forEach((heading) => {
      if (heading.textContent.trim() === "Are you sure you want to delete this?") {
        heading.textContent = "Confirma la eliminacion de este registro";
      }
    });
  }

  document.addEventListener("DOMContentLoaded", () => {
    polishTables();
    polishScaffoldText();
  });
})();
