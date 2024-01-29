using APICatalogo.Controllers;
using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace ApiCatalogoTests
{
    public class CategoriasControllerTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IMapper> _mapper;

        public CategoriasControllerTest()
        {
            _mapper = new Mock<IMapper>(MockBehavior.Default);
            _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Default);
        }

        [Fact]
        public void GetCategoriasMustReturnOK()
        {
            PagingParameters paginationParameters = new PagingParameters
            {
                PageNumber = 1,
                PageSize = 3,
            };

            List<Categoria> categorias = new List<Categoria>();
            categorias.Add(
                new Categoria
                {
                    CategoriaId = 1,
                    Name = "Test",
                    ImageUrl = "test.jpg"
                }
            );
            categorias.Add(
                new Categoria
                {
                    CategoriaId = 2,
                    Name = "Test2",
                    ImageUrl = "test2.jpg"
                }
            );
            categorias.Add(
                new Categoria
                {
                    CategoriaId = 3,
                    Name = "Test3",
                    ImageUrl = "test3.jpg"
                }
            );

            PagedList<Categoria> pagedList = new PagedList<Categoria>(categorias, 3, 1, 3);

            List<CategoriaDTO> categoriasDTO = new List<CategoriaDTO>();
            categoriasDTO.Add(
                    new CategoriaDTO
                    {
                        CategoriaId = 1,
                        Name = "Test",
                        ImageUrl = "test.jpg"
                    }
                );
            categoriasDTO.Add(
                    new CategoriaDTO
                    {
                        CategoriaId = 2,
                        Name = "Test2",
                        ImageUrl = "test2.jpg"
                    }
                );
            categoriasDTO.Add(
                    new CategoriaDTO
                    {
                        CategoriaId = 3,
                        Name = "Test3",
                        ImageUrl = "test3.jpg"
                    }
                );

            var httpContextMock = new Mock<HttpContext>();

            var controller = new CategoriasController(_unitOfWork.Object, _mapper.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContextMock.Object }
            };

            _unitOfWork.Setup(repo => repo.CategoriaRepository.GetCategorias(paginationParameters))
                          .Returns(pagedList);
            _mapper.Setup(setup => setup.Map<List<CategoriaDTO>>(It.IsAny<PagedList<Categoria>>())).Returns(categoriasDTO);

            httpContextMock.SetupGet(c => c.Response.Headers)
                           .Returns(new HeaderDictionary());

            var expectedResult = JsonConvert.SerializeObject(new
            {
                pagedList.TotalCount,
                pagedList.PageSize,
                pagedList.CurrentPage,
                pagedList.TotalPages,
                pagedList.HasNext,
                pagedList.HasPrevious
            });

            var resultado = controller.GetCategorias(paginationParameters);

            var okObjectResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var categoriasResult = Assert.IsType<List<CategoriaDTO>>(okObjectResult.Value);

            Assert.Equal(pagedList.Count, categoriasResult.Count);
            Assert.Equal(pagedList[0].CategoriaId, categoriasResult[0].CategoriaId);
            Assert.Equal(pagedList[1].CategoriaId, categoriasResult[1].CategoriaId);
            Assert.Equal(pagedList[2].CategoriaId, categoriasResult[2].CategoriaId);

            Assert.True(controller.Response.Headers.ContainsKey("X-Pagination"));
            Assert.Equal(expectedResult, controller.Response.Headers["X-Pagination"]);

            _mapper.Verify(mapper => mapper.Map<List<CategoriaDTO>>(pagedList), Times.Once);
        }

        [Fact]
        public void GetCategoriasMustReturnNotFound()
        {
            PagingParameters paginationParameters = new PagingParameters
            {
                PageNumber = 1,
                PageSize = 3,
            };

            _unitOfWork.Setup(setup => setup.CategoriaRepository.GetCategorias(paginationParameters))
                .Returns((PagedList<Categoria>)null);

            var results = new CategoriasController(_unitOfWork.Object, _mapper.Object).GetCategorias(paginationParameters);

            Assert.IsType<NotFoundObjectResult>(results.Result);
        }

        [Fact]
        public void GetCategoriaMustReturnOk()
        {
            Categoria categoria = new Categoria
            {
                CategoriaId = 1,
                Name = "test",
                ImageUrl = "test.jpg",
            };

            CategoriaDTO categoriaDTO = new CategoriaDTO
            {
                CategoriaId = 1,
                Name = "test",
                ImageUrl = "test.jpg"
            };

            _unitOfWork.Setup(setup => setup.CategoriaRepository.GetById(It.IsAny<Expression<Func<Categoria,bool>>>()))
                .Returns(categoria);

            _mapper.Setup(setup => setup.Map<CategoriaDTO>(It.IsAny<Categoria>())).Returns(categoriaDTO);

            var result = new CategoriasController(_unitOfWork.Object, _mapper.Object).GetCategoria(1);
            var okObjectResult = Assert.IsType<OkObjectResult>(result.Result);
            var categoriaRetornada = Assert.IsAssignableFrom<CategoriaDTO>(okObjectResult.Value);

            Assert.Equal(categoriaRetornada.CategoriaId, categoriaDTO.CategoriaId);
            Assert.Equal(categoriaRetornada.Name, categoriaDTO.Name);
            Assert.Equal(categoriaRetornada.ImageUrl, categoriaDTO.ImageUrl);
        }

        [Fact]
        public void GetCategoriaMustReturnNotFound()
        {
            _unitOfWork.Setup(setup => setup.CategoriaRepository.GetById(It.IsAny<Expression<Func<Categoria, bool>>>()))
                .Returns((Categoria) null);

            var result = new CategoriasController(_unitOfWork.Object, _mapper.Object).GetCategoria(1);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void GetCategoriaProdutosMustReturnOk()
        {
            List<Categoria> categorias = new List<Categoria>();
            categorias.Add(
                new Categoria
                {
                    CategoriaId = 1,
                    Name = "Test",
                    ImageUrl = "test.jpg",
                    Produtos = new List<Produto> { 
                        new Produto 
                        {  
                            ProdutoId=1, 
                            Name = "Test", 
                            Descricao = "Test",
                            ImageUrl = "test1.jpg",
                            DataCadastro = DateTime.Now,
                            Estoque = 3,
                            Preco = 50
                        } 
                    }
                }
            ); 
            categorias.Add(
                new Categoria
                {
                    CategoriaId = 2,
                    Name = "Test2",
                    ImageUrl = "test2.jpg",
                    Produtos = new List<Produto> {
                        new Produto
                        {
                            ProdutoId=2,
                            Name = "Test2",
                            Descricao = "Test2",
                            ImageUrl = "test2.jpg",
                            DataCadastro = DateTime.Now,
                            Estoque = 2,
                            Preco = 30
                        }
                    }
                }
            );

            List<CategoriaDTO> categoriasDTO = new List<CategoriaDTO>();
            categoriasDTO.Add(
                    new CategoriaDTO
                    {
                        CategoriaId = 1,
                        Name = "Test",
                        ImageUrl = "test.jpg",
                        Produtos = new List<ProdutoDTO> { 
                            new ProdutoDTO { 
                                ProdutoId=1,
                                Name = "Test",
                                Descricao = "Test",
                                ImageUrl = "test1.jpg",
                                Preco = 50
                            }
                        }
                    }
                );
            categoriasDTO.Add(
                    new CategoriaDTO
                    {
                        CategoriaId = 2,
                        Name = "Test2",
                        ImageUrl = "test2.jpg",
                        Produtos = new List<ProdutoDTO>
                        {
                            new ProdutoDTO
                            {
                                ProdutoId=2,
                                Name = "Test2",
                                Descricao = "Test2",
                                ImageUrl = "test2.jpg",
                                Preco = 30
                            }
                        }
                    }
                );

            _unitOfWork.Setup(setup => setup.CategoriaRepository.GetCategoriaProdutos())
                .Returns(categorias);
            _mapper.Setup(setup => setup.Map<List<CategoriaDTO>>(It.IsAny<List<Categoria>>())).Returns(categoriasDTO);

            var results = new CategoriasController(_unitOfWork.Object, _mapper.Object).GetCategoriaProdutos();
            var okObjectResult = Assert.IsType<OkObjectResult>(results.Result);
            var categoriasRetornadas = Assert.IsAssignableFrom<List<CategoriaDTO>>(okObjectResult.Value);

            Assert.Equal(categoriasRetornadas[0].CategoriaId, categorias.ElementAt(0).CategoriaId);
            Assert.Equal(categoriasRetornadas[0].Produtos.ElementAt(0).ProdutoId, categoriasDTO.ElementAt(0).Produtos.ElementAt(0).ProdutoId);
            Assert.Equal(categoriasRetornadas[1].CategoriaId, categorias.ElementAt(1).CategoriaId);
            Assert.Equal(categoriasRetornadas[0].Produtos.ElementAt(0).ProdutoId, categoriasDTO.ElementAt(0).Produtos.ElementAt(0).ProdutoId);
        }

        [Fact]
        public void CreateCategoriaTest()
        {
            var categoriaDto = new CategoriaDTO
            {
                Name = "Nova Categoria",
                ImageUrl = "teste.jpg"
            };

            var categoriaModel = new Categoria
            {
                CategoriaId = 1,
                Name = "Nova Categoria",
                ImageUrl = "teste.jpg"
            };

            _unitOfWork.Setup(repo => repo.CategoriaRepository.Add(It.IsAny<Categoria>()));
            _unitOfWork.Setup(repo => repo.Commit());

            _mapper.Setup(mapper => mapper.Map<Categoria>(categoriaDto))
                      .Returns(categoriaModel);

            _mapper.Setup(mapper => mapper.Map<CategoriaDTO>(categoriaModel))
                      .Returns(new CategoriaDTO
                      {
                          CategoriaId = categoriaModel.CategoriaId,
                          Name = categoriaModel.Name,
                          ImageUrl = categoriaDto.ImageUrl
                      });

            var controller = new CategoriasController(_unitOfWork.Object, _mapper.Object);
            var resultado = controller.CreateCategoria(categoriaDto);

            _unitOfWork.Verify(repo => repo.CategoriaRepository.Add(It.IsAny<Categoria>()), Times.Once);
            _unitOfWork.Verify(repo => repo.Commit(), Times.Once);

            var createdAtRouteResult = Assert.IsType<CreatedAtRouteResult>(resultado);
            var categoriaResponse = Assert.IsType<CategoriaDTO>(createdAtRouteResult.Value);
            Assert.Equal(categoriaDto.Name, categoriaResponse.Name);
            Assert.Equal(categoriaDto.ImageUrl, categoriaResponse.ImageUrl);
        }

        [Fact]
        public void CreateCategoriaMustReturnBadRequest()
        {
            var controller = new CategoriasController(_unitOfWork.Object, _mapper.Object);
            var resultado = controller.CreateCategoria(null);

            var createdAtRouteResult = Assert.IsType<BadRequestResult>(resultado);
        }

        [Fact]
        public void UpdateCategoriaMustReturnOk()
        {
            var categoriaDto = new CategoriaDTO
            {
                CategoriaId = 1,
                Name = "Categoria Atualizada",
                ImageUrl = "image.jpg"
            };

            var categoriaModel = new Categoria
            {
                CategoriaId = 1,
                Name = categoriaDto.Name,
                ImageUrl= categoriaDto.ImageUrl
            };

            _unitOfWork.Setup(repo => repo.CategoriaRepository.Update(It.IsAny<Categoria>()));
            _unitOfWork.Setup(repo => repo.Commit());

            _mapper.Setup(mapper => mapper.Map<Categoria>(categoriaDto))
                      .Returns(categoriaModel);

            var controller = new CategoriasController(_unitOfWork.Object, _mapper.Object);

            var resultado = controller.UpdateCategoria(1, categoriaDto);

            Assert.IsType<OkResult>(resultado);
            _unitOfWork.Verify(repo => repo.CategoriaRepository.Update(It.IsAny<Categoria>()), Times.Once);
            _unitOfWork.Verify(repo => repo.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateCategoriaMustReturnBadRequest()
        {
            var categoriaDto = new CategoriaDTO
            {
                CategoriaId = 2,
                Name = "Categoria Atualizada",
                ImageUrl = "image.jpg"
            };

            var categoriaModel = new Categoria
            {
                CategoriaId = 2,
                Name = categoriaDto.Name,
                ImageUrl = categoriaDto.ImageUrl
            };

            _mapper.Setup(mapper => mapper.Map<Categoria>(categoriaDto))
                      .Returns(categoriaModel);

            var controller = new CategoriasController(_unitOfWork.Object, _mapper.Object);

            var resultado = controller.UpdateCategoria(1, categoriaDto);

            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public void DeleteCategoriaMustReturnOk() 
        {
            var categoriaModel = new Categoria
            {
                CategoriaId = 1,
                Name = "Categoria Para Remover",
                ImageUrl = "remocao.jpg"
            };

            _unitOfWork.Setup(repo => repo.CategoriaRepository.GetById(It.IsAny<Expression<Func<Categoria, bool>>>()))
                          .Returns(categoriaModel);

            _unitOfWork.Setup(repo => repo.CategoriaRepository.Delete(It.IsAny<Categoria>()));
            _unitOfWork.Setup(repo => repo.Commit());

            _mapper.Setup(mapper => mapper.Map<CategoriaDTO>(categoriaModel))
                      .Returns(new CategoriaDTO
                      {
                          CategoriaId = categoriaModel.CategoriaId,
                          Name = categoriaModel.Name,
                          ImageUrl= categoriaModel.ImageUrl,
                      });

            var controller = new CategoriasController(_unitOfWork.Object, _mapper.Object);

            var resultado = controller.DeleteCategoria(1);

            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var categoriaResponse = Assert.IsType<CategoriaDTO>(okResult.Value);
            Assert.Equal(categoriaResponse.CategoriaId, categoriaModel.CategoriaId);
            Assert.Equal(categoriaResponse.Name, categoriaModel.Name);
            _unitOfWork.Verify(repo => repo.CategoriaRepository.Delete(It.IsAny<Categoria>()), Times.Once);
            _unitOfWork.Verify(repo => repo.Commit(), Times.Once);
        }

        [Fact]
        public void DeleteCategoriaMustReturnNotFound() 
        {
            _unitOfWork.Setup(repo => repo.CategoriaRepository.GetById(It.IsAny<Expression<Func<Categoria, bool>>>()))
                          .Returns((Categoria)null);

            var controller = new CategoriasController(_unitOfWork.Object, _mapper.Object);

            var resultado = controller.DeleteCategoria(1);

            var okResult = Assert.IsType<NotFoundObjectResult>(resultado);
        }
    }
}
