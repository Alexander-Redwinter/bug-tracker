document.addEventListener('DOMContentLoaded', () => {

    //card options
    const cardArray = [
        {
            name: 'alice',
            img: '/img/memory/alice.png'
        },
        {
            name: 'alice',
            img: '/img/memory/alice.png'
        },
        {
            name: 'marisa',
            img: '/img/memory/marisa.png'
        },
        {
            name: 'marisa',
            img: '/img/memory/marisa.png'
        },
        {
            name: 'remilia',
            img: '/img/memory/remilia.png'
        },
        {
            name: 'remilia',
            img: '/img/memory/remilia.png'
        },
        {
            name: 'flandre',
            img: '/img/memory/flandre.png'
        },
        {
            name: 'flandre',
            img: '/img/memory/flandre.png'
        },
        {
            name: 'cirno',
            img: '/img/memory/cirno.png'
        },
        {
            name: 'cirno',
            img: '/img/memory/cirno.png'
        },
        {
            name: 'reimu',
            img: '/img/memory/reimu.png'
        },
        {
            name: 'reimu',
            img: '/img/memory/reimu.png'
        },
        {
            name: 'aya',
            img: '/img/memory/aya.png'
        },
        {
            name: 'aya',
            img: '/img/memory/aya.png'
        },
    ]

    cardArray.sort(() => 0.5 - Math.random())

    var cardsChosen = []
    var cardsChosenId = []
    var cardsWon = []
var canChoose = true;
var turns = 0;




    const grid = document.querySelector('.memory-grid')
    const resultDisplay = document.querySelector('#result')

    function preloadImage(url) {
        var img = new Image();
        img.src = url;
    }




    //creating board
    function createBoard() {
        for (let i = 0; i < cardArray.length; i++) {
            preloadImage(cardArray[i].img)
            var card = document.createElement('img')
            card.setAttribute('src', '/img/memory/blank.png')
            card.setAttribute('height', 150)
            card.setAttribute('length', 150)

            card.setAttribute('data-id', i)
            card.addEventListener('click', flipCard)
            grid.appendChild(card)
        }
    }

    //check for matches
    function checkForMatch() {
        var cards = document.querySelectorAll('img')
        const optionOneId = cardsChosenId[0]
        const optionTwoId = cardsChosenId[1]
        if (cardsChosen[0] === cardsChosen[1]) {
            cards[optionOneId].setAttribute('src', '/img/memory/white.png')
            cards[optionTwoId].setAttribute('src', '/img/memory/white.png')
            cards[optionOneId].removeEventListener('click', flipCard)
            cards[optionTwoId].removeEventListener('click', flipCard)
            cardsWon.push(cardsChosen)

        } else {
            cards[optionOneId].setAttribute('src', '/img/memory/blank.png')
            cards[optionTwoId].setAttribute('src', '/img/memory/blank.png')
        }
        cardsChosen = []
        cardsChosenId = []
turns++;
        resultDisplay.textContent = turns;
canChoose = true;
        if (cardsWon.length === cardArray.length / 2) {
            resultDisplay.textContent = 'You won in ' + turns +' turns!'
        }
    }


    //flip card
    function flipCard() {
        var cardId = this.getAttribute('data-id')
        if (cardsChosenId.includes(cardId)) {
            return
        }
if (!canChoose){
return
}
        cardsChosen.push(cardArray[cardId].name)
        cardsChosenId.push(cardId)
        this.setAttribute('src', cardArray[cardId].img)
        if (cardsChosen.length === 2) {
canChoose = false;
            setTimeout(checkForMatch, 500)
        }
    }

    createBoard()
})