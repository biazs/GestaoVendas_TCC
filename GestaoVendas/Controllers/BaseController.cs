﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GestaoVendas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestaoVendas.Controllers
{
    public class BaseController : Controller
    {
        public async Task<bool> UsuarioTemAcesso(string pagina, GestaoVendasContext _context)
        {
            var usuario = User.Identity.Name;

            var temAcesso = await (from TP in _context.TipoUsuario
                                   join AT in _context.AcessoTipoUsuario on TP.Id equals AT.IdTipoUsuario
                                   join FU in _context.Funcionalidade on AT.IdFuncionalidade equals FU.Id
                                   join PF in _context.PerfilUsuario on TP.Id equals PF.IdTipoUsuario
                                   join US in _context.Usuario on PF.UserId equals US.Id
                                   where FU.NomeFuncionalidade == pagina && US.Email == usuario
                                   select new
                                   {
                                       TP.Id
                                   }).AnyAsync();

            return temAcesso;
        }
    }
}
