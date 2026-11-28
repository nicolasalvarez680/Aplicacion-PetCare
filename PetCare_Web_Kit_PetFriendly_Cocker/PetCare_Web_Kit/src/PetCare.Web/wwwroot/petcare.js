(() => {
    'use strict';
    const moneda = new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 2 });
    function actualizar(form) {
        if (form.matches('[data-cita-preview]')) {
            const duracion = Number(form.elements.duracion.value);
            form.querySelector('[data-cita-costo]').textContent = Number.isInteger(duracion) && duracion >= 15 && duracion <= 180 ? moneda.format(60000 + Math.max(0, duracion - 30) * 1500) : '—';
        }
        if (form.matches('[data-hospital-preview]')) {
            const entrada = form.elements.ingreso.value;
            const tarifa = Number(form.elements.tarifa.value);
            const fecha = new Date(entrada);
            const hoy = new Date();
            const dia = d => Date.UTC(d.getFullYear(), d.getMonth(), d.getDate());
            const valido = entrada && Number.isFinite(fecha.getTime()) && fecha <= hoy && tarifa > 0 && Number.isFinite(tarifa);
            const dias = valido ? Math.max(1, (dia(hoy) - dia(fecha)) / 86400000 + 1) : 0;
            form.querySelector('[data-dias]').textContent = valido ? String(dias) : '—';
            form.querySelector('[data-costo]').textContent = valido ? moneda.format(dias * tarifa) : '—';
        }
        if (form.matches('[data-invoice-preview]')) {
            let total = 0;
            for (const campo of form.querySelectorAll('[data-precio]')) {
                const cantidad = campo.type === 'checkbox' ? (campo.checked ? 1 : 0) : Number(campo.value);
                const precio = Number(campo.dataset.precio);
                if (Number.isFinite(cantidad) && cantidad > 0 && Number.isFinite(precio)) total += cantidad * precio;
            }
            form.querySelector('[data-total]').textContent = moneda.format(total);
        }
    }
    function actualizarMascota(form) {
        const tipo = form.querySelector('[data-tipo-mascota]');
        if (!tipo) return;
        const grupo = form.querySelector('[data-raza-grupo]');
        const raza = form.querySelector('[data-raza-mascota]');
        const perro = tipo.value === 'Perro';
        grupo.hidden = !perro;
        raza.required = perro;
        raza.disabled = !perro;
    }
    function iniciar() {
        for (const form of document.querySelectorAll('[data-hospital-preview], [data-invoice-preview], [data-cita-preview]')) actualizar(form);
        for (const tipo of document.querySelectorAll('[data-tipo-mascota]')) actualizarMascota(tipo.closest('form'));
    }
    document.addEventListener('input', e => { const form = e.target.closest('form'); if (form) actualizar(form); });
    document.addEventListener('change', e => { const form = e.target.closest('form'); if (form) { actualizar(form); if (e.target.matches('[data-tipo-mascota]')) actualizarMascota(form); } });
    document.addEventListener('click', e => { if (e.target.closest('[data-print]')) window.print(); });
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', iniciar); else iniciar();
    // Also works if an enhanced navigation is used in the future.
    if (window.Blazor?.addEventListener) window.Blazor.addEventListener('enhancedload', iniciar);
})();

// PetFriendly: controles de contraseña reales, sin dependencia de SignalR.
document.addEventListener('click', event => {
    const button = event.target.closest('[data-password-toggle]');
    if (!button) return;
    const input = document.getElementById(button.dataset.passwordToggle);
    if (!input) return;
    const visible = input.type === 'password';
    input.type = visible ? 'text' : 'password';
    button.setAttribute('aria-pressed', String(visible));
    button.setAttribute('aria-label', visible ? 'Ocultar contraseña' : 'Mostrar contraseña');
});
