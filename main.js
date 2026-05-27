let scene, camera, renderer, player;
let moveDirection = new THREE.Vector3();
let bots = [];
let isPlaying = false;

function init3D() {
    // 1. Setup 3D World Scene
    scene = new THREE.Scene();
    scene.background = new THREE.Color(0xcccccc);
    scene.fog = new THREE.FogExp2(0xcccccc, 0.015);

    camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
    renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setSize(window.innerWidth, window.innerHeight);
    document.body.appendChild(renderer.domElement);

    // 2. Add Lights & Ground Floor
    const light = new THREE.HemisphereLight(0xffffff, 0x444444, 1);
    scene.add(light);
    
    const floorGeo = new THREE.PlaneGeometry(100, 100);
    const floorMat = new THREE.MeshBasicMaterial({ color: 0x555555 });
    const floor = new THREE.Mesh(floorGeo, floorMat);
    floor.rotation.x = -Math.PI / 2;
    scene.add(floor);

    // 3. Create Player Object
    player = new THREE.Object3D();
    player.position.set(0, 2, 0);
    scene.add(player);
    player.add(camera); // Camera is attached inside the player head

    window.addEventListener('resize', onWindowResize);
    setupControls();
    animate();
}

// --- OP AIM ASSIST & AUTO SHOOT ---
function applyOPAimAssist() {
    let closestBot = null;
    let maxDegrees = 0.95; // Snappiness threshold

    bots.forEach(bot => {
        let botPos = bot.position.clone();
        let targetDir = botPos.sub(player.position).normalize();
        let cameraDir = new THREE.Vector3();
        camera.getWorldDirection(cameraDir);

        let dotProduct = cameraDir.dot(targetDir);
        if (dotProduct > maxDegrees) {
            closestBot = bot;
        }
    });

    if (closestBot) {
        // Aggressively snap camera look direction to target bot
        camera.lookAt(closestBot.position);
        
        // AUTO SHOOT LOGIC
        autoShootTarget(closestBot);
    }
}

function autoShootTarget(target) {
    // Deduct bot health instantly on crosshair collision
    if (!target.isDead) {
        target.health -= 1;
        target.material.color.setHex(0xff0000); // Flash red
        setTimeout(() => target.material.color.setHex(0x0000ff), 100);
        
        if (target.health <= 0) {
            target.isDead = true;
            scene.remove(target);
        }
    }
}

// --- LOBBY SYSTEMS ---
function hostGame() {
    document.getElementById("menu").style.display = "none";
    document.getElementById("ui-container").style.display = "block";
    init3D();
    spawnBots(5);
    isPlaying = true;
}

function joinGame() {
    hostGame(); // P2P fallthrough logic for browser demo
}

function spawnBots(count) {
    for(let i=0; i<count; i++) {
        let botGeo = new THREE.CapsuleGeometry(0.5, 1.5, 4, 8);
        let botMat = new THREE.MeshBasicMaterial({color: 0x0000ff});
        let bot = new THREE.Mesh(botGeo, botMat);
        bot.position.set(Math.random()*40 - 20, 1, Math.random()*40 - 20);
        bot.health = 100;
        bot.isDead = false;
        scene.add(bot);
        bots.push(bot);
    }
}

// --- INPUT HANDLERS & ANIMATION ---
function setupControls() {
    // Standard Keybinds (PC)
    window.addEventListener('keydown', (e) => {
        if(e.code === "KeyW") moveDirection.z = -1;
        if(e.code === "KeyS") moveDirection.z = 1;
        if(e.code === "KeyA") moveDirection.x = -1;
        if(e.code === "KeyD") moveDirection.x = 1;
    });
    window.addEventListener('keyup', (e) => {
        if(["KeyW", "KeyS"].includes(e.code)) moveDirection.z = 0;
        if(["KeyA", "KeyD"].includes(e.code)) moveDirection.x = 0;
    });
    
    // PC Pointer Lock for Mouse Control
    document.body.addEventListener('click', () => {
        if(isPlaying) document.body.requestPointerLock();
    });
    window.addEventListener('mousemove', (e) => {
        if (document.pointerLockElement === document.body) {
            player.rotation.y -= e.movementX * 0.002;
            camera.rotation.x -= e.movementY * 0.002;
        }
    });
}

function animate() {
    requestAnimationFrame(animate);
    if (!isPlaying) return;

    // Movement updates
    let speed = 0.1;
    player.translateOnAxis(moveDirection, speed);
    
    // Always run Aim Assist checks
    applyOPAimAssist();

    renderer.render(scene, camera);
}

function onWindowResize() {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
}