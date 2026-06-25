using SQLite;
using SonoGuard.Models;

namespace SonoGuard.Data
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _db;

        public DatabaseService()
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "sonoguard.db3");
            _db = new SQLiteAsyncConnection(path);
            _db.CreateTableAsync<Usuario>().Wait();
            _db.CreateTableAsync<Medicion>().Wait();
        }

        public Task<Usuario?> ObtenerUsuarioAsync(string email, string password)
            => _db.Table<Usuario>()
                  .Where(u => u.Email == email && u.Password == password)
                  .FirstOrDefaultAsync();

        public Task<Usuario?> ObtenerPorEmailAsync(string email)
            => _db.Table<Usuario>()
                  .Where(u => u.Email == email)
                  .FirstOrDefaultAsync();

        public Task<int> RegistrarUsuarioAsync(Usuario u) => _db.InsertAsync(u);

        public Task<int> GuardarMedicionAsync(Medicion m) => _db.InsertAsync(m);
        public Task<int> ActualizarMedicionAsync(Medicion m) => _db.UpdateAsync(m);
        public Task<int> EliminarMedicionAsync(Medicion m) => _db.DeleteAsync(m);

        public async Task<List<Medicion>> ObtenerMedicionesAsync(int usuarioId)
        {
            var lista = await _db.Table<Medicion>()
                                 .Where(m => m.UsuarioId == usuarioId)
                                 .ToListAsync();
            return lista.OrderByDescending(m => m.FechaHora).ToList();
        }

        public async Task<List<Medicion>> ObtenerDenunciasAsync(int usuarioId)
        {
            var lista = await _db.Table<Medicion>()
                                 .Where(m => m.UsuarioId == usuarioId && m.EsDenuncia)
                                 .ToListAsync();
            return lista.OrderByDescending(m => m.FechaHora).ToList();
        }
    }
}
