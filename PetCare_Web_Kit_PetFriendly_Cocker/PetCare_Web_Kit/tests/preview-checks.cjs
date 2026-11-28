// Pruebas de las vistas previas, sin dependencias externas (Node.js).
const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const path = require('node:path');
const moneda = new Intl.NumberFormat('es-CO', { style: 'currency', currency: 'COP', minimumFractionDigits: 2 });
function mock(tipo, elements, precios = []) {
  const out = {};
  return {out, elements, matches: s => s === `[data-${tipo}-preview]`, querySelector: s => out[s] ??= {textContent: ''}, querySelectorAll: () => precios};
}
let fecha = new Date(); fecha.setDate(fecha.getDate()-2);
const fechaLocal = `${fecha.getFullYear()}-${String(fecha.getMonth()+1).padStart(2,'0')}-${String(fecha.getDate()).padStart(2,'0')}T00:00`;
const h = mock('hospital',{ingreso:{value:fechaLocal},tarifa:{value:'80000'}});
const c = mock('cita',{duracion:{value:'60'}});
const f = mock('invoice',{},[{type:'checkbox',checked:true,dataset:{precio:'105000'}},{type:'number',value:'2',dataset:{precio:'15000'}}]);
const listeners = {};
const document = {readyState:'complete',querySelectorAll:sel=>sel==='[data-tipo-mascota]'?[]:[h,c,f],addEventListener:(key,fn)=>listeners[key]=fn};
vm.runInNewContext(fs.readFileSync(path.join(__dirname,'../src/PetCare.Web/wwwroot/petcare.js'),'utf8'), {document,window:{},Intl,Date,Number,Math});
assert.equal(h.out['[data-dias]'].textContent,'3');
assert.equal(h.out['[data-costo]'].textContent,moneda.format(240000));
assert.equal(c.out['[data-cita-costo]'].textContent,moneda.format(105000));
assert.equal(f.out['[data-total]'].textContent,moneda.format(135000));
h.elements.tarifa.value='0'; listeners.input({target:{closest:()=>h}});
assert.equal(h.out['[data-costo]'].textContent,'—');
console.log('5 comprobaciones de vistas previas correctas.');
// Especie y raza: comprobación de los controles y su estado de envío.
const tipo = {value:'Perro', matches:s=>s==='[data-tipo-mascota]', closest:()=>mascotaForm};
const grupo = {hidden:false};
const raza = {required:false,disabled:false,value:'Cocker Spaniel'};
const mascotaForm = {matches:()=>false,querySelector:s=>({'[data-tipo-mascota]':tipo,'[data-raza-grupo]':grupo,'[data-raza-mascota]':raza})[s]};
listeners.change({target:tipo}); assert.equal(grupo.hidden,false); assert.equal(raza.required,true); assert.equal(raza.disabled,false);
tipo.value='Gato'; listeners.change({target:tipo}); assert.equal(grupo.hidden,true); assert.equal(raza.required,false); assert.equal(raza.disabled,true);
tipo.value='Perro'; listeners.change({target:tipo}); assert.equal(grupo.hidden,false); assert.equal(raza.required,true); assert.equal(raza.disabled,false);
console.log('3 comprobaciones de tipo y raza correctas.');
