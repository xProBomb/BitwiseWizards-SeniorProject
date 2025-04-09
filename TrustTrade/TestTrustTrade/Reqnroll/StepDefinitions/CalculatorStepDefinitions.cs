using Reqnroll;

namespace TestTrustTrade.Reqnroll.StepDefinitions
{
    [Binding]
    public sealed class CalculatorStepDefinitions
    {
        private int _firstNumber;
        private int _secondNumber;
        private int _result;

        [Given("the first number is {int}")]
        public void GivenTheFirstNumberIs(int number)
        {
            _firstNumber = number;
        }

        [Given("the second number is {int}")]
        public void GivenTheSecondNumberIs(int number)
        {
            _secondNumber = number;
        }

        [When("I add the two numbers")]
        public void WhenTheTwoNumbersAreAdded()
        {
            _result = _firstNumber + _secondNumber;
        }

        [Then("the result should be {int}")]
        public void ThenTheResultShouldBe(int expectedResult)
        {
            Assert.That(_result, Is.EqualTo(expectedResult));
        }
    }
}