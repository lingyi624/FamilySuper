var VirtualSceneViewer = (function () {
    var scene, camera, renderer, model, raycaster, mouse;
    var annotations = [];
    var container;
    var isDragging = false;
    var previousMousePosition = { x: 0, y: 0 };
    var cameraDistance = 5;
    var cameraTheta = 0;
    var cameraPhi = Math.PI / 4;
    var onAnnotationClick = null;
    var onModelLoaded = null;

    function init(containerId) {
        container = document.getElementById(containerId);
        if (!container) return;

        scene = new THREE.Scene();
        scene.background = new THREE.Color(0xf0f0f0);

        camera = new THREE.PerspectiveCamera(75, container.clientWidth / container.clientHeight, 0.1, 1000);

        renderer = new THREE.WebGLRenderer({ antialias: true });
        renderer.setSize(container.clientWidth, container.clientHeight);
        renderer.setPixelRatio(window.devicePixelRatio);
        renderer.shadowMap.enabled = true;
        container.appendChild(renderer.domElement);

        var ambientLight = new THREE.AmbientLight(0xffffff, 0.6);
        scene.add(ambientLight);

        var directionalLight = new THREE.DirectionalLight(0xffffff, 0.8);
        directionalLight.position.set(5, 10, 5);
        directionalLight.castShadow = true;
        scene.add(directionalLight);

        var directionalLight2 = new THREE.DirectionalLight(0xffffff, 0.4);
        directionalLight2.position.set(-5, 5, -5);
        scene.add(directionalLight2);

        var gridHelper = new THREE.GridHelper(10, 10, 0xcccccc, 0xeeeeee);
        scene.add(gridHelper);

        var axesHelper = new THREE.AxesHelper(5);
        scene.add(axesHelper);

        raycaster = new THREE.Raycaster();
        mouse = new THREE.Vector2();

        updateCameraPosition();

        container.addEventListener('mousedown', onMouseDown);
        container.addEventListener('mousemove', onMouseMove);
        container.addEventListener('mouseup', onMouseUp);
        container.addEventListener('mousewheel', onMouseWheel);
        container.addEventListener('click', onClick);

        window.addEventListener('resize', onWindowResize);

        animate();
    }

    function updateCameraPosition() {
        camera.position.x = cameraDistance * Math.sin(cameraPhi) * Math.cos(cameraTheta);
        camera.position.y = cameraDistance * Math.cos(cameraPhi);
        camera.position.z = cameraDistance * Math.sin(cameraPhi) * Math.sin(cameraTheta);
        camera.lookAt(0, 0, 0);
    }

    function onMouseDown(event) {
        isDragging = true;
        previousMousePosition = {
            x: event.clientX,
            y: event.clientY
        };
    }

    function onMouseMove(event) {
        if (!isDragging) return;

        var deltaMove = {
            x: event.clientX - previousMousePosition.x,
            y: event.clientY - previousMousePosition.y
        };

        cameraTheta += deltaMove.x * 0.01;
        cameraPhi += deltaMove.y * 0.01;
        cameraPhi = Math.max(0.1, Math.min(Math.PI - 0.1, cameraPhi));

        updateCameraPosition();
        previousMousePosition = { x: event.clientX, y: event.clientY };
    }

    function onMouseUp() {
        isDragging = false;
    }

    function onMouseWheel(event) {
        cameraDistance += event.deltaY * 0.01;
        cameraDistance = Math.max(1, Math.min(20, cameraDistance));
        updateCameraPosition();
    }

    function onClick(event) {
        if (!container || !model) return;

        var rect = container.getBoundingClientRect();
        mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
        mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

        raycaster.setFromCamera(mouse, camera);
        var intersects = raycaster.intersectObject(model, true);

        if (intersects.length > 0 && onAnnotationClick) {
            var point = intersects[0].point;
            onAnnotationClick({
                x: point.x,
                y: point.y,
                z: point.z,
                faceNormal: {
                    x: intersects[0].face.normal.x,
                    y: intersects[0].face.normal.y,
                    z: intersects[0].face.normal.z
                }
            });
        }
    }

    function onWindowResize() {
        if (!container) return;
        camera.aspect = container.clientWidth / container.clientHeight;
        camera.updateProjectionMatrix();
        renderer.setSize(container.clientWidth, container.clientHeight);
    }

    function animate() {
        requestAnimationFrame(animate);
        renderer.render(scene, camera);
    }

    function loadModel(modelPath) {
        if (model) {
            scene.remove(model);
            model = null;
        }

        const loader = new THREE.GLTFLoader();
        loader.load(modelPath, function (gltf) {
            model = gltf.scene;
            model.traverse(function (child) {
                if (child.isMesh) {
                    child.castShadow = true;
                    child.receiveShadow = true;
                }
            });

            var box = new THREE.Box3().setFromObject(model);
            var center = box.getCenter(new THREE.Vector3());
            var size = box.getSize(new THREE.Vector3());
            var maxDim = Math.max(size.x, size.y, size.z);
            var scale = 3 / maxDim;

            model.position.sub(center);
            model.scale.multiplyScalar(scale);

            scene.add(model);

            if (onModelLoaded) {
                onModelLoaded({
                    center: { x: center.x, y: center.y, z: center.z },
                    size: { x: size.x, y: size.y, z: size.z },
                    scale: scale
                });
            }
        }, undefined, function (error) {
            console.error('Error loading model:', error);
        });
    }

    function addAnnotation(id, position, text, color) {
        var annotationGroup = new THREE.Group();

        var sphereGeometry = new THREE.SphereGeometry(0.08, 16, 16);
        var sphereMaterial = new THREE.MeshBasicMaterial({ color: color || 0xff0000 });
        var marker = new THREE.Mesh(sphereGeometry, sphereMaterial);
        marker.position.set(position.x, position.y, position.z);
        annotationGroup.add(marker);

        var pulseGeometry = new THREE.RingGeometry(0.1, 0.15, 32);
        var pulseMaterial = new THREE.MeshBasicMaterial({
            color: color || 0xff0000,
            transparent: true,
            opacity: 0.5,
            side: THREE.DoubleSide
        });
        var pulse = new THREE.Mesh(pulseGeometry, pulseMaterial);
        pulse.rotation.x = -Math.PI / 2;
        pulse.position.set(position.x, position.y, position.z);
        annotationGroup.add(pulse);

        var canvas = document.createElement('canvas');
        var ctx = canvas.getContext('2d');
        canvas.width = 256;
        canvas.height = 64;
        ctx.fillStyle = 'rgba(0, 0, 0, 0.8)';
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        ctx.fillStyle = '#ffffff';
        ctx.font = '16px Arial';
        ctx.fillText(text, 10, 35);

        var texture = new THREE.CanvasTexture(canvas);
        var spriteMaterial = new THREE.SpriteMaterial({ map: texture });
        var sprite = new THREE.Sprite(spriteMaterial);
        sprite.scale.set(1.5, 0.38, 1);
        sprite.position.set(position.x, position.y + 0.2, position.z);
        annotationGroup.add(sprite);

        annotationGroup.userData = { id: id, text: text };
        annotations.push(annotationGroup);
        scene.add(annotationGroup);

        return annotationGroup;
    }

    function removeAnnotation(id) {
        var index = annotations.findIndex(a => a.userData.id === id);
        if (index !== -1) {
            scene.remove(annotations[index]);
            annotations.splice(index, 1);
        }
    }

    function clearAnnotations() {
        annotations.forEach(a => scene.remove(a));
        annotations = [];
    }

    function dispose() {
        if (container && renderer) {
            container.removeEventListener('mousedown', onMouseDown);
            container.removeEventListener('mousemove', onMouseMove);
            container.removeEventListener('mouseup', onMouseUp);
            container.removeEventListener('mousewheel', onMouseWheel);
            container.removeEventListener('click', onClick);
            window.removeEventListener('resize', onWindowResize);

            if (renderer.domElement) {
                container.removeChild(renderer.domElement);
            }
            renderer.dispose();
        }
        scene = null;
        camera = null;
        renderer = null;
        model = null;
    }

    return {
        init: init,
        loadModel: loadModel,
        addAnnotation: addAnnotation,
        removeAnnotation: removeAnnotation,
        clearAnnotations: clearAnnotations,
        dispose: dispose,
        setOnAnnotationClick: function (callback) { onAnnotationClick = callback; },
        setOnModelLoaded: function (callback) { onModelLoaded = callback; },
        getAnnotations: function () { return annotations.map(a => ({ id: a.userData.id, text: a.userData.text })); }
    };
})();