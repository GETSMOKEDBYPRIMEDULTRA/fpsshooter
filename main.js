let scene, camera, renderer, player;
let moveDirection = new THREE.Vector3();
let bots = [];
let isPlaying = false;
let keys = {};

// We use a clean block design to guarantee everything renders immediately
function init3D() {
    // 1. Create Scene with a solid Blue Sky background (No black screen allowed!)
    scene = new THREE.Scene();
    scene.background = new THREE.Color(0x87CEEB); 

    camera = new THREE.PerspectiveCamera(75, window.innerWidth / window.innerHeight, 0.1, 1000);
    
    renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setSize(window.innerWidth, window.innerHeight);
    document.body.appendChild(renderer.domElement);

    // 2. Strong, bright lighting
    const ambientLight = new THREE.AmbientLight(0xffffff, 0.6);
    scene.add(ambientLight);
    const dirLight = new THREE.DirectionalLight(0xffffff, 0.8);
    dirLight.position.set(10, 20, 10);
    scene.add(dirLight);
    
    // 3. Bright Ground Floor Grid so you can see movement
    const floorGeo = new THREE.PlaneGeometry(200, 200);
    const floorMat = new THREE.MeshStandardMaterial({ color: 0x228B22 }); // Grass Green
    const floor = new THREE.Mesh(floorGeo, floorMat);
    floor.rotation.x = -Math.PI / 2;
    scene.add(floor);

    const grid = new THREE.GridHelper(200, 50, 0x000000, 0xffffff);
    grid.position.y = 0.01;
    scene.add(grid);

    // 4. Create Player & position clearly above ground
    player = new THREE.Object3D();
    player.position.set(0, 2, 0);
    scene.add(player);
    player.add(camera); 

    setupControls();
    spawnBots(8);
    animate();
}

// --- TARGET RED BLOCKS TO SHOOT ---
function spawnBots(count) {
    for(let i=0; i<count; i++) {
        // Red boxes make easy targets
        let botGeo = new THREE.BoxGeometry(1.5, 3, 1.5);
        let botMat = new THREE.MeshStandardMaterial({color: 0xff0000}); 
        let bot = new THREE.Mesh(botGeo, botMat);
        
        // Scatter them in front of the player
        bot.position.set(Math.random()*40 - 20, 1.5, Math.random()* -30 - 10);
        bot.health = 100;
        bot.isDead = false;
        scene.add(bot);
        bots.push(bot);
    }
}

// --- OP AIM ASSIST & AUTO-SHOOT ---
function applyOPAimAssist() {
    let closestBot = null;
    let maxDegrees = 0.96; 

    bots.forEach(bot => {
        if (bot.isDead) return;
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
        // Aggressively snap camera view to target box
        camera.lookAt(closestBot.position);
        
        // Auto Shoot
        closestBot.health -= 2;
        closestBot.material.color.setHex(0xffff00); // Flashes Yellow when shot
        
        if (closestBot.health <= 0) {
            closestBot.isDead = true;
            scene.remove(closestBot);
        }
    } else {
        // Reset bot colors back to red if not targeted
        bots.forEach(b => { if(!b.isDead) b.material.color.setHex(0xff0000); });
    }
}

// --- SYSTEM HANDLERS ---
function hostGame() {
    document.getElementById("menu").style.display = "none";
    document.getElementById("ui-container").style.display = "block";
    init3D();
    isPlaying = true;
}

function joinGame() {
    hostGame(); 
}

function setupControls() {
    window.addEventListener('keydown', (e) => { keys[e.code] = true; });
    window.addEventListener('keyup', (e) => { keys[e.code] = false; });
    
    document.body.addEventListener('click', () => {
        if(isPlaying) document.body.requestPointerLock();
    });
    
    window.addEventListener('mousemove', (e) => {
        if (document.pointerLockElement === document.body) {
            player.rotation.y -= e.movementX * 0.0025;
            camera.rotation.x -= e.movementY * 0.0025;
            camera.rotation.x = Math.max(-Math.PI/2.5, Math.min(Math.PI/2.5, camera.rotation.x));
        }
    });
}

function animate() {
    requestAnimationFrame(animate);
    if (!isPlaying) return;

    // Smooth movement processing
    let speed = 0.15;
    if (keys['KeyW'] || keys['ArrowUp']) player.translateOnAxis(new THREE.Vector3(0,0,-1), speed);
    if (keys['KeyS'] || keys['ArrowDown']) player.translateOnAxis(new THREE.Vector3(0,0,1), speed);
    if (keys['KeyA'] || keys['ArrowLeft']) player.translateOnAxis(new THREE.Vector3(-1,0,0), speed);
    if (keys['KeyD'] || keys['ArrowRight']) player.translateOnAxis(new THREE.Vector3(1,0,0), speed);
    
    // Lock position to floor height
    player.position.y = 2;

    applyOPAimAssist();
    renderer.render(scene, camera);
}

window.addEventListener('resize', () => {
    if(!camera || !renderer) return;
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
});
