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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ApiCatalogoTests
{
    public class ProdutosControllerTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IMapper> _mapper;

        public ProdutosControllerTest()
        {
            _mapper = new Mock<IMapper>(MockBehavior.Default);
            _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Default);
        }

        [Fact]
        public void GetMustReturnProdutosDTO()
        {
            var pagingParameters = new PagingParameters
            {
                PageNumber = 1,
                PageSize = 3,
            };

            var produtos = new PagedList<Produto>(new List<Produto>
            {
                new Produto { ProdutoId = 1, Name = "Produto 1", ImageUrl = "produto1.jpg", DataCadastro = DateTime.Now, Descricao = "Produto 1", Estoque = 1, Preco = 10, CategoriaId = 1},
                new Produto { ProdutoId = 2, Name = "Produto 2", ImageUrl = "produto2.jpg", DataCadastro = DateTime.Now, Descricao = "Produto 2", Estoque = 2, Preco = 20, CategoriaId = 2},
                new Produto { ProdutoId = 3, Name = "Produto 3", ImageUrl = "produto3.jpg", DataCadastro = DateTime.Now, Descricao = "Produto 3", Estoque = 3, Preco = 30, CategoriaId = 3}
            }, 3, 1, 3);

            var produtosDto = new List<ProdutoDTO>
            {
                new ProdutoDTO { ProdutoId = 1, Name = "Produto 1", ImageUrl = "produto1.jpg", Descricao = "Produto 1", Preco = 10, CategoriaId = 1},
                new ProdutoDTO { ProdutoId = 2, Name = "Produto 2", ImageUrl = "produto2.jpg", Descricao = "Produto 2", Preco = 20, CategoriaId = 2},
                new ProdutoDTO { ProdutoId = 3, Name = "Produto 3", ImageUrl = "produto3.jpg", Descricao = "Produto 3", Preco = 30, CategoriaId = 3}
            };

            var httpContextMock = new Mock<HttpContext>();

            var controller = new ProdutosController(_unitOfWork.Object, _mapper.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = httpContextMock.Object }
            };

            _unitOfWork.Setup(repo => repo.ProdutoRepository.GetProdutos(pagingParameters))
                          .Returns(produtos);
            _mapper.Setup(setup => setup.Map<List<ProdutoDTO>>(It.IsAny<PagedList<Produto>>())).Returns(produtosDto);

            httpContextMock.SetupGet(c => c.Response.Headers)
                           .Returns(new HeaderDictionary());

            var expectedResult = JsonConvert.SerializeObject(new
            {
                produtos.TotalCount,
                produtos.PageSize,
                produtos.CurrentPage,
                produtos.TotalPages,
                produtos.HasNext,
                produtos.HasPrevious
            });

            var resultado = controller.Get(pagingParameters);

            var okObjectResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var produtosResult = Assert.IsType<List<ProdutoDTO>>(okObjectResult.Value);

            Assert.Equal(produtos.Count, produtosResult.Count);
            Assert.Equal(produtos[0].ProdutoId, produtosResult[0].ProdutoId);
            Assert.Equal(produtos[1].ProdutoId, produtosResult[1].ProdutoId);
            Assert.Equal(produtos[2].ProdutoId, produtosResult[2].ProdutoId);

            Assert.True(controller.Response.Headers.ContainsKey("X-Pagination"));
            Assert.Equal(expectedResult, controller.Response.Headers["X-Pagination"]);

            _mapper.Verify(mapper => mapper.Map<List<ProdutoDTO>>(produtos), Times.Once);
        }

        [Fact]
        public void GetProdutosMustReturnNotFound()
        {
            PagingParameters paginationParameters = new PagingParameters
            {
                PageNumber = 1,
                PageSize = 3,
            };

            _unitOfWork.Setup(setup => setup.ProdutoRepository.GetProdutos(paginationParameters))
                .Returns((PagedList<Produto>)null);

            var results = new ProdutosController(_unitOfWork.Object, _mapper.Object).Get(paginationParameters);

            Assert.IsType<NotFoundObjectResult>(results.Result);
        }

        [Fact]
        public void GetProdutosPrecoMustReturnProdutosDTO()
        {
            var produtos = new List<Produto>
            {
                new Produto { ProdutoId = 1, Name = "Produto 1", ImageUrl = "produto1.jpg", DataCadastro = DateTime.Now, Descricao = "Produto 1", Estoque = 1, Preco = 10, CategoriaId = 1},
                new Produto { ProdutoId = 2, Name = "Produto 2", ImageUrl = "produto2.jpg", DataCadastro = DateTime.Now, Descricao = "Produto 2", Estoque = 2, Preco = 20, CategoriaId = 2},
                new Produto { ProdutoId = 3, Name = "Produto 3", ImageUrl = "produto3.jpg", DataCadastro = DateTime.Now, Descricao = "Produto 3", Estoque = 3, Preco = 30, CategoriaId = 3}
            };

            var produtosDto = new List<ProdutoDTO>
            {
                new ProdutoDTO { ProdutoId = 1, Name = "Produto 1", ImageUrl = "produto1.jpg", Descricao = "Produto 1", Preco = 10, CategoriaId = 1},
                new ProdutoDTO { ProdutoId = 2, Name = "Produto 2", ImageUrl = "produto2.jpg", Descricao = "Produto 2", Preco = 20, CategoriaId = 2},
                new ProdutoDTO { ProdutoId = 3, Name = "Produto 3", ImageUrl = "produto3.jpg", Descricao = "Produto 3", Preco = 30, CategoriaId = 3}
            };

            _unitOfWork.Setup(repo => repo.ProdutoRepository.GetProdutosPorPreco())
                          .Returns(produtos.AsQueryable());

            _mapper.Setup(mapper => mapper.Map<List<ProdutoDTO>>(It.IsAny<IEnumerable<Produto>>()))
                      .Returns(produtosDto);

            var controller = new ProdutosController(_unitOfWork.Object, _mapper.Object);
            var resultado = controller.GetProdutosPreco();

            var okObjectResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var produtosDTO = Assert.IsType<List<ProdutoDTO>>(okObjectResult.Value);

            Assert.Equal(produtos.Count, produtosDTO.Count);
            Assert.Equal(produtos[0].ProdutoId, produtosDTO[0].ProdutoId);
            Assert.Equal(produtos[1].ProdutoId, produtosDTO[1].ProdutoId);
            Assert.Equal(produtos[2].ProdutoId, produtosDTO[2].ProdutoId);

            _mapper.Verify(mapper => mapper.Map<List<ProdutoDTO>>(It.IsAny<IEnumerable<Produto>>()), Times.Once);
        }

        [Fact]
        public void GetProductMustReturnOk()
        {
            Produto produto =
                new Produto
                {
                    ProdutoId = 1,
                    Name = "Produto 1",
                    ImageUrl = "produto1.jpg",
                    DataCadastro = DateTime.Now,
                    Descricao = "Produto 1",
                    Estoque = 1,
                    Preco = 10,
                    CategoriaId = 1
                };

            ProdutoDTO produtoDto =
                new ProdutoDTO
                {
                    ProdutoId = 1,
                    Name = "Produto 1",
                    ImageUrl = "produto1.jpg",
                    Descricao = "Produto 1",
                    Preco = 10,
                    CategoriaId = 1
                };

            _unitOfWork.Setup(setup => setup.ProdutoRepository.GetById(It.IsAny<Expression<Func<Produto, bool>>>()))
                .Returns(produto);

            _mapper.Setup(setup => setup.Map<ProdutoDTO>(It.IsAny<Produto>())).Returns(produtoDto);

            var result = new ProdutosController(_unitOfWork.Object, _mapper.Object).GetProduct(1);
            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            var produtoRetornado = Assert.IsAssignableFrom<ProdutoDTO>(okObjectResult.Value);

            Assert.Equal(produtoRetornado.Name, produtoDto.Name);
            Assert.Equal(produtoRetornado.Descricao, produtoDto.Descricao);
            Assert.Equal(produtoRetornado.Preco, produtoDto.Preco);
        }

        [Fact]
        public void GetProductMustReturnNotFound()
        {
            _unitOfWork.Setup(setup => setup.ProdutoRepository.GetById(It.IsAny<Expression<Func<Produto, bool>>>()))
                .Returns((Produto)null);

            var result = new ProdutosController(_unitOfWork.Object, _mapper.Object).GetProduct(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void CreateProductTest()
        {
            Produto produto =
                new Produto
                {
                    ProdutoId = 1,
                    Name = "Produto 1",
                    ImageUrl = "produto1.jpg",
                    DataCadastro = DateTime.Now,
                    Descricao = "Produto 1",
                    Estoque = 1,
                    Preco = 10,
                    CategoriaId = 1
                };

            ProdutoDTO produtoDto =
                new ProdutoDTO
                {
                    ProdutoId = 1,
                    Name = "Produto 1",
                    ImageUrl = "produto1.jpg",
                    Descricao = "Produto 1",
                    Preco = 10,
                    CategoriaId = 1
                };

            _unitOfWork.Setup(repo => repo.ProdutoRepository.Add(It.IsAny<Produto>()));
            _unitOfWork.Setup(repo => repo.Commit());

            _mapper.Setup(mapper => mapper.Map<Produto>(produtoDto))
                      .Returns(produto);

            _mapper.Setup(mapper => mapper.Map<ProdutoDTO>(produto))
                      .Returns(produtoDto);

            var controller = new ProdutosController(_unitOfWork.Object, _mapper.Object);
            var resultado = controller.CreateProduct(produtoDto);

            _unitOfWork.Verify(repo => repo.ProdutoRepository.Add(It.IsAny<Produto>()), Times.Once);
            _unitOfWork.Verify(repo => repo.Commit(), Times.Once);

            var createdAtRouteResult = Assert.IsType<CreatedAtRouteResult>(resultado);
            var produtoResponse = Assert.IsType<ProdutoDTO>(createdAtRouteResult.Value);
            Assert.Equal(produtoDto.ProdutoId, produtoResponse.ProdutoId);
            Assert.Equal(produtoDto.Name, produtoResponse.Name);
            Assert.Equal(produtoDto.ImageUrl, produtoResponse.ImageUrl);
            Assert.Equal(produtoDto.Preco, produtoResponse.Preco);
        }

        [Fact]
        public void CreateProductMustReturnBadRequest()
        {
            var controller = new ProdutosController(_unitOfWork.Object, _mapper.Object);
            var resultado = controller.CreateProduct(null);

            var createdAtRouteResult = Assert.IsType<BadRequestResult>(resultado);
        }

        [Fact]
        public void UpdateProductMustReturnOk()
        {
            Produto produto =
               new Produto
               {
                   ProdutoId = 1,
                   Name = "Produto 1",
                   ImageUrl = "produto1.jpg",
                   DataCadastro = DateTime.Now,
                   Descricao = "Produto 1",
                   Estoque = 1,
                   Preco = 10,
                   CategoriaId = 1
               };

            ProdutoDTO produtoDto =
                new ProdutoDTO
                {
                    ProdutoId = 1,
                    Name = "Produto 1",
                    ImageUrl = "produto1.jpg",
                    Descricao = "Produto 1",
                    Preco = 10,
                    CategoriaId = 1
                };

            _unitOfWork.Setup(repo => repo.ProdutoRepository.Update(It.IsAny<Produto>()));
            _unitOfWork.Setup(repo => repo.Commit());

            _mapper.Setup(mapper => mapper.Map<Produto>(produtoDto))
                      .Returns(produto);

            var controller = new ProdutosController(_unitOfWork.Object, _mapper.Object);

            var resultado = controller.UpdateProduct(1, produtoDto);

            Assert.IsType<OkResult>(resultado);
            _unitOfWork.Verify(repo => repo.ProdutoRepository.Update(It.IsAny<Produto>()), Times.Once);
            _unitOfWork.Verify(repo => repo.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateProductMustReturnBadRequest()
        {
            Produto produto =
               new Produto
               {
                   ProdutoId = 2,
                   Name = "Produto 1",
                   ImageUrl = "produto1.jpg",
                   DataCadastro = DateTime.Now,
                   Descricao = "Produto 1",
                   Estoque = 1,
                   Preco = 10,
                   CategoriaId = 1
               };

            ProdutoDTO produtoDto =
                new ProdutoDTO
                {
                    ProdutoId = 2,
                    Name = "Produto 1",
                    ImageUrl = "produto1.jpg",
                    Descricao = "Produto 1",
                    Preco = 10,
                    CategoriaId = 1
                };

            _mapper.Setup(mapper => mapper.Map<Produto>(produtoDto))
                      .Returns(produto);

            var controller = new ProdutosController(_unitOfWork.Object, _mapper.Object);

            var resultado = controller.UpdateProduct(1, produtoDto);

            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public void DeleteProductMustReturnOk()
        {
            Produto produto =
               new Produto
               {
                   ProdutoId = 1,
                   Name = "Produto 1",
                   ImageUrl = "produto1.jpg",
                   DataCadastro = DateTime.Now,
                   Descricao = "Produto 1",
                   Estoque = 1,
                   Preco = 10,
                   CategoriaId = 1
               };

            _unitOfWork.Setup(repo => repo.ProdutoRepository.GetById(It.IsAny<Expression<Func<Produto, bool>>>()))
                          .Returns(produto);

            _unitOfWork.Setup(repo => repo.ProdutoRepository.Delete(It.IsAny<Produto>()));
            _unitOfWork.Setup(repo => repo.Commit());

            _mapper.Setup(mapper => mapper.Map<ProdutoDTO>(produto))
                      .Returns(new ProdutoDTO
                      {
                          ProdutoId = 1,
                          Name = "Produto 1",
                          ImageUrl = "produto1.jpg",
                          Descricao = "Produto 1",
                          Preco = 10,
                          CategoriaId = 1
                      });

            var controller = new ProdutosController(_unitOfWork.Object, _mapper.Object);

            var resultado = controller.DeleteProduct(1);

            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var produtoResponse = Assert.IsType<ProdutoDTO>(okResult.Value);
            Assert.Equal(produtoResponse.ProdutoId, produto.ProdutoId);
            Assert.Equal(produtoResponse.Name, produto.Name);
            _unitOfWork.Verify(repo => repo.ProdutoRepository.Delete(It.IsAny<Produto>()), Times.Once);
            _unitOfWork.Verify(repo => repo.Commit(), Times.Once);
        }

        [Fact]
        public void DeleteProductMustReturnNotFound()
        {
            _unitOfWork.Setup(repo => repo.ProdutoRepository.GetById(It.IsAny<Expression<Func<Produto, bool>>>()))
                          .Returns((Produto)null);

            var controller = new ProdutosController(_unitOfWork.Object, _mapper.Object);

            var resultado = controller.DeleteProduct(1);

            var okResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
        }
    }
}
