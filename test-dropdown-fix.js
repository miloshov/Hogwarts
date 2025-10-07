const axios = require('axios');

const BASE_URL = 'http://localhost:5241';
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

let token = '';

async function login() {
    try {
        console.log(`🔐 Login...`);
        const response = await axios.post(`${BASE_URL}/api/auth/login`, {
            username: 'admin',
            password: 'admin123'
        });
        
        token = response.data.Token || response.data.token;
        console.log(`✅ Login uspešan!`);
        return true;
    } catch (error) {
        console.log(`❌ Login greška: ${error.message}`);
        return false;
    }
}

async function testDropdownEndpoint() {
    try {
        console.log(`\n🔍 Testiram NOVI dropdown endpoint...`);
        const response = await axios.get(`${BASE_URL}/api/zaposleni/dropdown`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        console.log(`✅ Dropdown API poziv uspešan!`);
        console.log(`\n📋 RESPONSE STRUKTURA:`);
        console.log(`==============================================`);
        console.log(JSON.stringify(response.data, null, 2));
        console.log(`==============================================`);
        
        console.log(`\n🔍 ANALIZA:`);
        console.log(`Type: ${typeof response.data}`);
        console.log(`Is Array: ${Array.isArray(response.data)}`);
        console.log(`Length: ${response.data?.length || 'N/A'}`);
        
        if (Array.isArray(response.data) && response.data.length > 0) {
            console.log(`\n✅ PERFEKTNO! Dropdown će raditi!`);
            console.log(`\n👥 Zaposleni za dropdown:`);
            response.data.forEach((emp, index) => {
                console.log(`  ${index + 1}. ${emp.punoIme} (${emp.pozicija}) - ID: ${emp.id}`);
            });
        } else {
            console.log(`\n❌ Problem: prazan niz ili pogrešan format`);
        }
        
        return true;
    } catch (error) {
        console.log(`❌ Greška dropdown endpoint: ${error.message}`);
        if (error.response) {
            console.log(`Status: ${error.response.status}`);
            console.log(`Response:`, error.response.data);
        }
        return false;
    }
}

async function compareEndpoints() {
    try {
        console.log(`\n🔄 Poređenje sa starim endpoint-om...`);
        
        // Test starog endpoint-a
        const oldResponse = await axios.get(`${BASE_URL}/api/zaposleni`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        console.log(`\nSTARI /api/zaposleni:`);
        console.log(`- Tip: ${typeof oldResponse.data}`);
        console.log(`- Struktura: ${Array.isArray(oldResponse.data) ? 'Array' : 'Object sa keys: ' + Object.keys(oldResponse.data).join(', ')}`);
        
        // Test novog endpoint-a
        const newResponse = await axios.get(`${BASE_URL}/api/zaposleni/dropdown`, {
            headers: { 'Authorization': `Bearer ${token}` }
        });
        
        console.log(`\nNOVI /api/zaposleni/dropdown:`);
        console.log(`- Tip: ${typeof newResponse.data}`);
        console.log(`- Struktura: ${Array.isArray(newResponse.data) ? 'Array' : 'Object'}`);
        console.log(`- Length: ${newResponse.data?.length || 'N/A'}`);
        
        console.log(`\n🎯 ZAKLJUČAK: Novi endpoint je ${Array.isArray(newResponse.data) ? '✅ SAVRŠEN za dropdown!' : '❌ još uvek problematičan'}`);
        
    } catch (error) {
        console.log(`❌ Greška pri poređenju: ${error.message}`);
    }
}

async function runDropdownTest() {
    console.log('🎯 TEST NOVOG DROPDOWN ENDPOINT-a!\n');
    
    const loginOK = await login();
    if (!loginOK) return;
    
    await testDropdownEndpoint();
    await compareEndpoints();
    
    console.log('\n🏁 Test završen!');
}

runDropdownTest().catch(console.error);