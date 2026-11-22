# GitHub Desktop: primer commit de PetCare

## Error `Permission denied` en `.vs/*.vsidx`
Visual Studio mantiene algunos archivos de índice dentro de `.vs` y puede bloquearlos. No se deben subir al repositorio.

1. Guarda el trabajo y cierra Visual Studio.
2. Comprueba que el `.gitignore` de la raíz contenga:

```gitignore
.vs/
**/.vs/
[Bb]in/
[Oo]bj/
*.user
*.suo
*.userosscache
*.sln.docstates
*.tmp
*.log
.DS_Store
Thumbs.db
```

3. Abre la terminal desde GitHub Desktop: **Repository > Open in Terminal**.
4. Desde la raíz del repositorio, ejecuta el comando ajustando la ruta si corresponde:

```bash
git rm -r --cached --ignore-unmatch "Aplicacion PetCare/.vs"
```

5. Regresa a GitHub Desktop. Verifica que el código fuente, `.csproj`, `.sln`/`.slnx`, configuraciones no secretas y migraciones sigan incluidos, pero que `.vs`, `bin` y `obj` no aparezcan para commit.
6. Haz el commit y luego **Push origin**.

La advertencia LF/CRLF es de finales de línea y normalmente no es la causa del fallo. No se debe ignorar un error `Permission denied`; se corrige excluyendo `.vs` y liberando el archivo bloqueado.

## No subir secretos
No incluyas contraseñas, claves, cadenas de conexión con credenciales ni secretos reales. Usa configuración local o User Secrets para valores privados.
