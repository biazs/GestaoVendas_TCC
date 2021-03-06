﻿using System.Linq;
using System.Threading.Tasks;
using GestaoVendas.Data;
using GestaoVendas.Libraries.Mensagem;
using GestaoVendas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GestaoVendas.Controllers
{
    public class PerfilUsuariosController : BaseController
    {
        private readonly GestaoVendasContext _context;

        public PerfilUsuariosController(GestaoVendasContext context)
        {
            _context = context;
        }

        // GET: PerfilUsuarios
        public async Task<IActionResult> Index()
        {
            var gestaoVendasContext = _context.PerfilUsuario.Include(p => p.ItentityUser).Include(p => p.TipoUsuario).OrderBy(p => p.TipoUsuario);
            return View(await gestaoVendasContext.ToListAsync());
        }

        // GET: PerfilUsuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var perfilUsuario = await _context.PerfilUsuario
                .Include(p => p.ItentityUser)
                .Include(p => p.TipoUsuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (perfilUsuario == null)
            {
                return NotFound();
            }

            return View(perfilUsuario);
        }

        // GET: PerfilUsuarios/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
            ViewData["IdTipoUsuario"] = new SelectList(_context.TipoUsuario, "Id", "NomeTipoUsuario");
            return View();
        }

        // POST: PerfilUsuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string nome_vendedor, [Bind("Id,IdTipoUsuario,UserId")] PerfilUsuario perfilUsuario)
        {
            if (ModelState.IsValid)
            {
                //Verificar se perfil Usuario já existe
                if (PerfilUsuarioExistsTipo(perfilUsuario.IdTipoUsuario, perfilUsuario.UserId))
                {
                    TempData["MSG_E"] = Mensagem.MSG_E010;
                    return RedirectToAction(nameof(Create));
                }

                _context.Add(perfilUsuario);

                //inserir na tabela Vendedor
                if (nome_vendedor != null && nome_vendedor != "" && _context.TipoUsuario.Any(v => v.Id == perfilUsuario.IdTipoUsuario && (v.NomeTipoUsuario == "Vendedor" || v.NomeTipoUsuario == "vendedor")))
                {
                    var email = _context.Users.FirstOrDefault(u => u.Id == perfilUsuario.UserId).Email;
                    var vendedor = new Vendedor() { Nome = nome_vendedor, Email = email, UserId = perfilUsuario.UserId };
                    _context.Vendedor.Add(vendedor);
                }

                await _context.SaveChangesAsync();
                TempData["MSG_S"] = Mensagem.MSG_S001;
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", perfilUsuario.UserId);
            ViewData["IdTipoUsuario"] = new SelectList(_context.TipoUsuario, "Id", "NomeTipoUsuario", perfilUsuario.IdTipoUsuario);
            return View(perfilUsuario);
        }

        // GET: PerfilUsuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var perfilUsuario = await _context.PerfilUsuario.FindAsync(id);
            if (perfilUsuario == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", perfilUsuario.UserId);
            ViewData["IdTipoUsuario"] = new SelectList(_context.TipoUsuario, "Id", "NomeTipoUsuario", perfilUsuario.IdTipoUsuario);
            return View(perfilUsuario);
        }

        // POST: PerfilUsuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdTipoUsuario,UserId")] PerfilUsuario perfilUsuario)
        {
            if (id != perfilUsuario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(perfilUsuario);
                    await _context.SaveChangesAsync();
                    TempData["MSG_S"] = Mensagem.MSG_S001;
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PerfilUsuarioExists(perfilUsuario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", perfilUsuario.UserId);
            ViewData["IdTipoUsuario"] = new SelectList(_context.TipoUsuario, "Id", "NomeTipoUsuario", perfilUsuario.IdTipoUsuario);
            return View(perfilUsuario);
        }

        // GET: PerfilUsuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var perfilUsuario = await _context.PerfilUsuario
                .Include(p => p.ItentityUser)
                .Include(p => p.TipoUsuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (perfilUsuario == null)
            {
                return NotFound();
            }

            return View(perfilUsuario);
        }

        // POST: PerfilUsuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var perfilUsuario = await _context.PerfilUsuario.FindAsync(id);
            _context.PerfilUsuario.Remove(perfilUsuario);

            //deletar Vendedor da tabela
            var isVendedor = _context.TipoUsuario.Any(v => v.Id == perfilUsuario.IdTipoUsuario && (v.NomeTipoUsuario == "Vendedor" || v.NomeTipoUsuario == "vendedor"));
            var existeVendedor = _context.Vendedor.Any(v => v.UserId == perfilUsuario.UserId);

            if (isVendedor && existeVendedor)
            {
                var email = _context.Users.FirstOrDefault(u => u.Id == perfilUsuario.UserId).Email;
                var vendedor = _context.Vendedor.FirstOrDefault(v => v.Email == email && v.UserId == perfilUsuario.UserId);

                _context.Vendedor.Remove(vendedor);
            }

            await _context.SaveChangesAsync();
            TempData["MSG_S"] = Mensagem.MSG_S002;
            return RedirectToAction(nameof(Index));
        }

        private bool PerfilUsuarioExists(int id)
        {
            return _context.PerfilUsuario.Any(e => e.Id == id);
        }

        private bool PerfilUsuarioExistsTipo(int tipoUsuario, string userId)
        {
            return _context.PerfilUsuario.Any(e => e.IdTipoUsuario == tipoUsuario && e.UserId == userId);
        }
    }
}
