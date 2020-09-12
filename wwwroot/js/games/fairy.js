

document.addEventListener('DOMContentLoaded', () => {

    const squares = document.querySelectorAll('.fairy-grid div');
    const resultDisplay = document.querySelector('#result');

    let width = 15;
    let currentReimuIndex = 202;
    let currentFairyIndex = 0;
    let fairiesDown = []
    let result = 0
    let direction = 1
    let fairyId
    var shootCooldown = 400;
    const startSpeed = 1500;
    const minSpeed = 150;
    var speedUp = 50;
    var speed = startSpeed;
    var inPlay = true;
    const fairies = [
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
        15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
        30, 31, 32, 33, 34, 35, 36, 37, 38, 39
    ]


    //for (let i = 0; i < 30; i++) {
    //        fairies.push(i);
    //}

    fairies.forEach(fairy => squares[currentFairyIndex + fairy].classList.add('fairy'));

    squares[currentReimuIndex].classList.add('reimu');

    function moveReimu(e) {
        squares[currentReimuIndex].classList.remove('reimu');

        switch (e.keyCode) {

            case 37:
                if (currentReimuIndex % width !== 0)
                    currentReimuIndex -= 1
                break;
            case 39:
                if (currentReimuIndex % width < width - 1)
                    currentReimuIndex += 1
                break;
        }

        squares[currentReimuIndex].classList.add('reimu');

    }

    document.addEventListener('keydown', moveReimu)


    function moveFairies() {
        const leftEdge = fairies[0] % width === 0
        const rightEdge = fairies[fairies.length - 1] % width === width - 1

        if ((leftEdge && direction === -1) || (rightEdge && direction === 1)) {
            direction = width

        } else if (direction === width) {
            if (leftEdge)
                direction = 1;
            else
                direction = -1;
        }
        for (let i = 0; i <= fairies.length - 1; i++) {
            squares[fairies[i]].classList.remove('fairy')
        }
        for (let i = 0; i <= fairies.length - 1; i++) {
            fairies[i] += direction;
        }
        for (let i = 0; i <= fairies.length - 1; i++) {
            if (!fairiesDown.includes(i)) {
                squares[fairies[i]].classList.add('fairy')

            }
        }


        if (squares[currentReimuIndex].classList.contains('fairy', 'reimu')) {
            gameOver();
        }

        for (let i = 0; i <= fairies.length - 1; i++) {
            if (fairies[i] > (squares.length - (width - 1))) {
                gameOver();

            }
        }

        if (fairiesDown.length === fairies.length) {
            result.textContent = "you win gratz"
            clearTimeout(fairyId)
            inPlay = false;
        }
    }




    function loop() {
        if (inPlay) {
            moveFairies()
            speed = startSpeed - fairiesDown.length * speedUp
            if (speed < minSpeed)
                speed = minSpeed;
            fairyId = setTimeout(loop, speed)
        }
    }

    var canShoot = true;

    function shoot(e) {
            let ofudaId;
            let currentOfudaId = currentReimuIndex;



        function moveOfuda() {
            squares[currentOfudaId].classList.remove('bullet')
            currentOfudaId -= width;
            squares[currentOfudaId].classList.add('bullet')
            if (squares[currentOfudaId].classList.contains('fairy')) {

                squares[currentOfudaId].classList.remove('bullet')
                squares[currentOfudaId].classList.remove('fairy')
                squares[currentOfudaId].classList.add('boom')

                setTimeout(() => squares[currentOfudaId].classList.remove('boom'),150)
                clearInterval(ofudaId)

                const fairyDown = fairies.indexOf(currentOfudaId)
                fairiesDown.push(fairyDown);
                result++;
                resultDisplay.textContent = result;
            }



            if (currentOfudaId < width) {
                clearInterval(ofudaId)
                squares[currentOfudaId].classList.remove('bullet');
            }
        }

        if (canShoot) {
            switch (e.keyCode) {
                case 32:
                    canShoot = false;
                    ofudaId = setInterval(moveOfuda, 100)
                    setTimeout(() => { canShoot = true }, shootCooldown)
                    break
            }

        }

    }

    document.addEventListener('keyup', shoot);

    function gameOver() {
        squares[currentReimuIndex].classList.add('boom');
        resultDisplay.textContent = 'game over m8 game over'
        inPlay = false;

        clearTimeout(fairyId)
    }


    loop()
});