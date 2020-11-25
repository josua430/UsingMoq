using System;
using Services;
using Moq;
using NUnit.Framework;
using api.Controllers;
using System.Linq;
using System.Collections.Generic;

namespace api.test
{
    public class CartControllerTest
    {
        private CartController controller;
        private Mock<IPaymentService> paymentServiceMock;
        private Mock<ICartService> cartServiceMock;

        private Mock<IShipmentService> shipmentServiceMock;
        private Mock<ICard> cardMock;
        private Mock<IAddressInfo> addressInfoMock;
        private List<CartItem> items;

        [SetUp]
        public void Setup()
        {
            //crea un objeto mock, se usa la interfaz para el mock, no la implementacion
            cartServiceMock = new Mock<ICartService>();
            paymentServiceMock = new Mock<IPaymentService>();
            shipmentServiceMock = new Mock<IShipmentService>();

            // organizacion de objetos que se utilizan
            cardMock = new Mock<ICard>();
            addressInfoMock = new Mock<IAddressInfo>();

            // define items mock del carrito de compra
            var cartItemMock = new Mock<CartItem>();
            cartItemMock.Setup(item => item.Price).Returns(10);

            items = new List<CartItem>()
            {
                cartItemMock.Object
            };
            //configura que se devuelvan los items mock cuando se soliciten
            cartServiceMock.Setup(c => c.Items()).Returns(items.AsEnumerable());
            //creaci�n de objeto de controller con los mock
            controller = new CartController(cartServiceMock.Object, paymentServiceMock.Object, shipmentServiceMock.Object);

        }

        [Test]
        public void ShouldReturnCharged()
        {

            //var card = new Card("owner", "number", "CVV number");
            //paymentServiceMock.Setup(p => p.Charge(114, card)).Returns(true)

            //se configura el mock para que al ejecutar el m�todo Charge retorne true
            //el helper It permite usar cualquier valor de un tipo de datos. En este caso se usa para pasar
            //cualquier valor double
            //se configura el mock para que al ejecutar el m�todo Charge retorne true. El m�todo recibe como
            //par�metro un double y una implementacion de ICard (cardMock.Object)
            paymentServiceMock.Setup(p => p.Charge(It.IsAny<double>(), cardMock.Object)).Returns(true);

            // ejecuci�n de m�todo para probar
            var result = controller.CheckOut(cardMock.Object, addressInfoMock.Object);

            // validacion
            // myInterfaceMock.Verify((m => m.DoesSomething()), Times.Once());
            shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Once());

            Assert.AreEqual("charged", result);
        }

        [Test]
        public void ShouldReturnNotCharged()
        {
            //se configura el mock para que al ejecutar el m�todo Charge retorne false
            //el helper It permite usar cualquier valor de un tipo de datos. En este caso se usa para pasar
            //cualquier valor double
            //se configura el mock para que al ejecutar el m�todo Charge retorne true. El m�todo recibe como
            //par�metro un double y una implementacion de ICard (cardMock.Object)
            paymentServiceMock.Setup(p => p.Charge(It.IsAny<double>(), cardMock.Object)).Returns(false);

            // ejecucion del m�todo a probar con los mock
            var result = controller.CheckOut(cardMock.Object, addressInfoMock.Object);

            // validaci�n
            shipmentServiceMock.Verify(s => s.Ship(addressInfoMock.Object, items.AsEnumerable()), Times.Never());
            Assert.AreEqual("not charged", result);
        }
    }
}